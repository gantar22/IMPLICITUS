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
        LayoutRebuilder.MarkLayoutForRebuild(transform.parent.GetComponent<RectTransform>());
        
        if (target.goal.Equal(term))
        {
            Success.Invoke();
        }
        Debug.Log(arg_paren);
        Destroy(arg_paren);
        variable_symbols_here.gameObject.SetActive(false);
        foreach (var canvase in FindObjectsOfType<Canvas>())
        {
            LayoutRebuilder.MarkLayoutForRebuild(canvase.GetComponent<RectTransform>());
        }
    }

    public IEnumerator Skip()
    {  
        IEnumerator succ()
        {
            yield return new WaitForSeconds(.6f);
            if (!Util.CanEvaluate(term, new List<int>(), (v, rule) => rule).Any())
                button.image.enabled = false;
            else
                yield return Skip();
        }
        yield return StepRoutine(succ(),() => {});
    }

    public void Step()
    {
        IEnumerator succ()
        {
            if (!Util.CanEvaluate(term, new List<int>(), (v, rule) => rule).Any())
                button.image.enabled = false;
            yield return new WaitForSeconds(.15f);
            button.interactable = true;
            yield return null;
        }
        
        button.interactable = false;
        StartCoroutine(StepRoutine(succ(), () =>
        {
            //TODO (<<) reverse all the way here
        }));
    }
    public IEnumerator StepRoutine(IEnumerator succ, Action fail)
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

    private void OnDestroy()
    {
        cleanup();
    }
}
