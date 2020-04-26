using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Lambda;
using TMPro;
using TypeUtil;
using UnityEngine;
using UnityEngine.UI;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator,Lambda.Variable>>;

public class SymbolManagerTester : MonoBehaviour
{
    [SerializeField] private string input;
    [SerializeField] private SymbolManager manager;
    
    [SerializeField] private List<Combinator> combinators;
    [field: SerializeField] public List<char> Variables { get; set; }
    public LayoutTracker currentLayout;


    private void Start()
    {
        currentLayout = manager.Initialize(Term.Node(new List<Term>())).GetComponentInChildren<LayoutTracker>();
		resetParensAlpha(currentLayout);

		Destroy(currentLayout.GetComponent<DraggableSpell>());
    }

    public void reset()
    {
        Destroy(currentLayout.gameObject);
        currentLayout = manager.Initialize(Term.Node(new List<Term>()));
		resetParensAlpha(currentLayout);
	}

    public Term CreateTerm(string s)
    {
        string temp = input;
        input = s;
        CreateTerm();
        Term result = GetTerm();
        input = temp;
        return result;
    }
    
    public string show(Term t)
    {
        return t.Map<string>(v => v.Match(c => c.ToString(), i => Variables[(int) i].ToString())).ToString();
    }

    [ContextMenu("CreateTerm")]
    public void CreateTerm()
    {
        currentLayout = CreateTermHelper().Item2;
    }


    private Term GetTerm()
    {
        var coms = combinators.ToDictionary(c => c.info.nameInfo.name);
        (List<Shrub<char>> input_shrub, var empty) =
            Lambda.Util.ParseParens(input.Where(c => !char.IsWhiteSpace(c)).ToList(), c => c.Equals('('), c => c.Equals(')'));

        if (empty.Count != 0)
        {
            throw new ArgumentException();
        }
        
        return Shrub<char>.Node(input_shrub).Map<Sum<Combinator,Lambda.Variable>>(
            ch =>
            {
                if (coms.ContainsKey(ch))
                {
                    return Sum<Combinator, Variable>.Inl(coms[ch]);
                }
                else
                {
                    if(Variables.IndexOf(ch) == -1) print($"variable: {ch}, in term: {input_shrub}, vars: {Variables.Select(x => $"{x}").Aggregate((a,b) => $"{a}, {b}")}");
                    return Sum<Combinator, Variable>.Inr((Variable)Variables.IndexOf(ch));
                }
            }
        );
    }
    
    private Tuple<Term,LayoutTracker> CreateTermHelper()
    {

        var input_term = GetTerm();
        symbol = manager.Initialize(input_term);
		resetParensAlpha(symbol);
		return Tuple.Create(input_term,symbol);
    }

    public void CreateTerm(Term t)
    {
		if (currentLayout && currentLayout.gameObject) {
			Destroy(currentLayout.gameObject);
		}
        currentLayout = manager.Initialize(t);
		resetParensAlpha(currentLayout);
	}
    


    private LayoutTracker symbol;


    [ContextMenu("STEP")]
    public void step()
    {
        var term = GetTerm();
        var rules = Lambda.Util.CanEvaluate(term,new List<int>(),(v,rule) => rule);
        
        
        /*foreach (var elimRule in rules)
        {
            
            if (elimRule is CombinatorElim CElim)
            {
                if (CElim.Target().Count == 0)
                {
                    print($"    Combinator: {CElim.c} at top");
                }
                else
                {
                    print($"    Combinator: {CElim.c} at {CElim.Target().Select(i => i.ToString()).Aggregate((a, b) => $"{a} {b}")}");
                }
            } else if (elimRule is ParenElim PElim)
            {
                if (PElim.Target().Count == 0)
                {
                    print("    Paren elim at top");
                }
                else
                {
                    print($"    Paren at {PElim.Target().Select(i => i.ToString()).Aggregate((a,b) => $"{a} {b}")}");
                }
            }
        }*/

        /*if(rules.Count > 0 && rules[0] is ParenElim parenElim)
        {
            input = show(rules[0].evaluate(term));
            input = String.Concat(input.Skip(1).Take(input.Length - 2).ToList());
            rules = Lambda.Util.CanEvaluate(rules[0].evaluate(term), new List<int>(), (_, rule) => rule);
            return;
        }*/

        if (rules.Count == 0)
        {
            return;
        }
        else
        {
            StartCoroutine(manager.Transition(rules[0], symbol));
            input = show(rules[0].evaluate(term));
            input = String.Concat(input.Skip(1).Take(input.Length - 2).ToList());
            
        }

    }

	// Set the alpha of the currentLayout parens (top level parens) to 0
	private void resetParensAlpha(LayoutTracker layoutTracker) {
		Image image = layoutTracker.GetComponent<Image>();
		Color c = image.color;
		image.color = new Color(c.r, c.g, c.b, .0f);
	}
}
