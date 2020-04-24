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

    [SerializeField]
    Button button;


    [SerializeField] IntEvent effectAudioEvent; //Event Calls audio sound

    private Term term;
    
    
    private void Awake()
    {
        cleanup = arityEvent.AddRemovableListener(i => arity = i);
        button.onClick.AddListener(Cast);
    }

    public void UnCast()
    {
        onUnapply.Invoke();
        button.onClick.RemoveListener(Step);
        button.onClick.AddListener(Cast);
        button_name.text = "Go";
    }
    
    public void Cast()
    {
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
        button_name.text = "Step";
        
        if (target.goal.Equal(term))
        {
            Success.Invoke();
        }
        Debug.Log(arg_paren);
        Destroy(arg_paren);
        foreach (var canvase in FindObjectsOfType<Canvas>())
        {
            LayoutRebuilder.MarkLayoutForRebuild(canvase.GetComponent<RectTransform>());
        }
    }

    public void Step()
    {
        var rules = Lambda.Util.CanEvaluate(term,new List<int>(),(v,rule) => rule);
        if (rules.Count == 0)
        {
            return;
        }
        else
		{
			pushUndoProposalTerm.Invoke(term);
			proposal.Transition(term, rules[0], proposal.GetComponentInChildren<LayoutTracker>());
            term = rules[0].evaluate(term);
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
