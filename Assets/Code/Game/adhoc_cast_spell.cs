using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lambda;
using TMPro;
using TypeUtil;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator,Lambda.Variable>>;

public class adhoc_cast_spell : MonoBehaviour
{
    [SerializeField]
    private Transform variable_symbols_here;

    [SerializeField] private Sprite castSymbol;
    [SerializeField] private Sprite stepSymbol;
    [SerializeField] private Button SkipButton;
    [SerializeField] private Button SkipBackButton;
    [SerializeField] private Button StepBackButton;
    [SerializeField]
    private SymbolManager proposal;
    [FormerlySerializedAs("goal")] [SerializeField]
    private SpawnTarget target;
    [SerializeField]
    private IntEvent arityEvent;
    private int arity;
    private Action cleanup;
    [SerializeField]
    private TMP_Text button_name;
	[SerializeField]
	private TermEvent pushUndoProposalTerm;

    [SerializeField] private UnitEvent onApply;
    [SerializeField] private UnitEvent onUnapply;

    [SerializeField] private UnitEvent Success;
    [SerializeField] private BoolRef evalmode;

    [SerializeField]
    Button button;


    [SerializeField] IntEvent effectAudioEvent; //Event Calls audio sound

    private Term term;
    
    
    private void Awake()
    {
        cleanup = arityEvent.AddRemovableListener(i => arity = i);
        button.onClick.AddListener(Cast);
        SkipButton.onClick.AddListener(() => StartCoroutine(Skip()));
        StepBackButton.onClick.AddListener(StepBack);
        SkipBackButton.onClick.AddListener(() => StartCoroutine(SkipBack()));
        evalmode.val = false;
    }

    public void Update()
    {
        if(!evalmode.val)
            button.interactable = 1 < proposal.GetComponentsInChildren<LayoutTracker>().Length;
    }

    public void UnCast()
    {
        evalmode.val = false;
        variable_symbols_here.gameObject.SetActive(true);
        onUnapply.Invoke();
        button.onClick.RemoveListener(Step);
        button.onClick.AddListener(Cast);
        button.image.sprite = castSymbol;
        button.image.enabled = true;
        
        SkipButton.gameObject.SetActive(false);
        SkipBackButton.gameObject.SetActive(false);
        StepBackButton.gameObject.SetActive(false);
        LayoutRebuilder.MarkLayoutForRebuild(transform.parent.GetComponent<RectTransform>());
    }
    
    public void Cast()
    {
        evalmode.val = true;
        onApply.Invoke();
        effectAudioEvent.Invoke(7); //Cast Spell Sound

        List<Term> args = Enumerable.Range(0, arity)
            .Select(i => Term.Leaf(Sum<Combinator, Variable>.Inr((Variable) i))).ToList();
        proposal.Append(args);

        LayoutTracker arg_paren = variable_symbols_here.GetComponentInChildren<LayoutTracker>();


        List<Transform> ts = new List<Transform>();
        for (int i = 0; i < arg_paren.transform.childCount; i++)
        {
            ts.Add(arg_paren.transform.GetChild(i));
        }

        foreach(Transform variable in ts)
        {
            variable.GetComponent<LayoutTracker>().root = proposal.skeletonRoot;
            variable.SetParent(proposal.GetComponentInChildren<LayoutTracker>().transform,true);
        }

        term = proposal.readTerm();
        
        button.onClick.RemoveListener(Cast);
        button.onClick.AddListener(Step);
        button.GetComponent<Image>().sprite = stepSymbol;
        SkipButton.gameObject.SetActive(true);
        SkipBackButton.gameObject.SetActive(true);
        StepBackButton.gameObject.SetActive(true);
        LayoutRebuilder.MarkLayoutForRebuild(transform.parent.GetComponent<RectTransform>());
        
        if (target.goal.Equal(term))
        {
            Success.Invoke();
        }
        Debug.Log(arg_paren);
        Destroy(arg_paren);
        variable_symbols_here.gameObject.SetActive(false);
        foreach (var canvas in FindObjectsOfType<Canvas>())
        {
            LayoutRebuilder.MarkLayoutForRebuild(canvas.GetComponent<RectTransform>());
        }
    }

    public IEnumerator Skip()
    {  
        IEnumerator succ()
        {
            yield return new WaitForSeconds(.6f);
            if (Util.CanEvaluate(term, new List<int>(), (v, rule) => rule).Any())
                yield return Skip();
            else
                refreshButtons();
        }

        SkipButton.interactable = false;
        yield return StepRoutine(succ(),() => {});
    }

    public void Step()
    {
        IEnumerator succ()
        {
            yield return new WaitForSeconds(.15f);
            refreshButtons();
            yield return null;
        }
        
        button.interactable = false;
        StartCoroutine(StepRoutine(succ(), refreshButtons));
    }
    IEnumerator StepRoutine(IEnumerator succ, Action fail)
    {
        var rules = Lambda.Util.CanEvaluate(term,new List<int>(),(v,rule) => rule);
        if (rules.Count == 0)
        {
            fail();
            yield break;
        }
        else
		{
			pushUndoProposalTerm.Invoke(term);
            yield return proposal.Transition(rules[0], proposal.GetComponentInChildren<LayoutTracker>());
            term = rules[0].evaluate(term);


            yield return succ;            
            if (target.goal.Equal(term))
            {
                Success.Invoke();
            }
        }
    }

    IEnumerator SkipBack()
    {
        IEnumerator succ()
        {
            yield return new WaitForSeconds(.5f);
            if (proposal.HasBackStack())
                yield return SkipBack();
            else
                refreshButtons();
        }

        StepBackButton.interactable = false;
        yield return StepBackRoutine(succ(), refreshButtons);
    }
    
    public void StepBack()
    {
        IEnumerator succ()
        {
            yield return new WaitForSeconds(.15f);
            refreshButtons();
        }

        StepBackButton.interactable = false;
        StartCoroutine(StepBackRoutine(succ(), () => { }));
    }

    IEnumerator StepBackRoutine(IEnumerator succ, Action fail)
    {
        if (!proposal.HasBackStack())
        {
            fail();
            yield break;
        }
        yield return proposal.popBackwards();
        yield return succ;
    }

    void refreshButtons()
    {
        StepBackButton.interactable = proposal.HasBackStack();
        SkipBackButton.interactable = proposal.HasBackStack();
        button.interactable = Util.CanEvaluate(term, new List<int>(), (v, rule) => rule).Any();
        SkipButton.interactable = Util.CanEvaluate(term, new List<int>(), (v, rule) => rule).Any();
    }

    private void OnDestroy()
    {
        cleanup();
    }
}
