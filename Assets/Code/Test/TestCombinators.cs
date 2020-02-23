using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Lambda;
using UnityEngine;
using UnityEngine.UI;
using TypeUtil;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator,Lambda.Variable>>;

public class TestCombinators : MonoBehaviour
{
    [SerializeField] private List<Combinator> combinators;
    [SerializeField] private InputField inputField;
    [SerializeField] private VerticalLayoutGroup outputGroup;
    [SerializeField] private Text text_prefab;
    [SerializeField] private List<char> variables;


    public void TesttoString()
    {
        var b = Sum<Combinator,Variable>.Inl(combinators[0]);
        var t = Sum<Combinator,Variable>.Inl(combinators[1]);
        var x = Sum<Combinator, Variable>.Inr(0);
        var y = Sum<Combinator, Variable>.Inr((Variable)1);
        print(Term.Node(new List<Term>().Append(Term.Leaf(b)).Append(Term.Leaf(t)).Append(Term.Node(new List<Term>().Append(Term.Leaf(x)).Append(Term.Leaf(y)).ToList())).ToList()));
    }


    public string show(Term t)
    {
        return t.Map<string>(v => v.Match(c => c.ToString(), i => variables[(int) i].ToString())).ToString();
    }
    
    public void Test() //call from button
    {
        var coms = combinators.ToDictionary(c => c.info.nameInfo.name);
        
        (List<Shrub<char>> input_shrub, var empty) =
            Lambda.Util.ParseParens(inputField.text.ToList(), c => c.Equals('('), c => c.Equals(')'));

        if (empty.Count != 0)
        {
            throw new ArgumentException();
        }
        
        
       
        
        Term input_term = Shrub<char>.Node(input_shrub).Map<Sum<Combinator,Lambda.Variable>>(
            ch =>
                {
                    if (coms.ContainsKey(ch))
                    {
                        return Sum<Combinator, Variable>.Inl(coms[ch]);
                    }
                    else
                    {
                        return Sum<Combinator, Variable>.Inr((Variable)variables.IndexOf(ch));
                    }
                }
            );
        List<ElimRule> rules = Util.CanEvaluate(input_term, new List<int>(), (t, rule) => rule);
        int cancel = 500;
        
        print($"Total term: {input_term}");
        while (rules.Count != 0 && input_term.IsNode() && 0 < cancel)
        {
            cancel--;
            Instantiate(text_prefab, outputGroup.transform).text = show(input_term);
            input_term = rules[0].evaluate(input_term);
            
            print($"Total term: {input_term}");
            rules = Util.CanEvaluate(input_term, new List<int>(), (t, rule) => rule);
        }
        
        Instantiate(text_prefab, outputGroup.transform).text = show(input_term);
        
    }

    
    
}
