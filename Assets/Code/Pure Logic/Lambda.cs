using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using TypeUtil;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator,Lambda.Variable>>;

namespace Lambda
{

    public enum Variable : int {}
    
    

    public interface ElimRule
    {
        List<int> Target();
        Shrub<T> evaluate<T>(Shrub<T> Term);
    }

    public class ParenElim : ElimRule
    {
        private readonly List<int> path;
        
        public ParenElim(List<Term> term, List<Term> leftElement, List<int> path)
        {
            this.path = path.ToList();
            if (term.Count == 0 || term[0].Match(l => l != leftElement, v => true))
            {
                //throw new Exception($"Term: {term.Select(x => x.ToString()).Aggregate((l,r) => $"{l}, {r}")}.\n LeftElement: {leftElement.Select(x => x.ToString()).Aggregate((l,r) => $"{l}, {r}")}.");
            }
        }

        public List<int> Target()
        {
            return path.ToList();
        }

        public Shrub<T> evaluate<T>(Shrub<T> term)
        {
            return term.ApplyAt(t => t.Match(
                l => l[0].Match(
                    leftelement => Shrub<T>.Node(leftelement.Concat(l.Skip(1)).ToList()),
                    x => throw new Exception() //doesn't begin with parens
                ),
                x =>
                {
                    Debug.Log($" path: {path.Select(i => $"{i}").Aggregate((l,r) => $"{l},{r}")}");
                    throw new Exception();
                }
//isn't even an application
            ),path);
        }
    }


    public class CombinatorElim : ElimRule
    {
        public readonly Combinator c;

        private readonly List<int> path;

        public CombinatorElim(Combinator c, List<Term> term,List<int> path)
        {
            if (term[0].Match(l => true, sum => sum.Match(c2 => c2 != c, v => true)))
            {
                throw new ArgumentException();
            }
            this.c = c;
            this.path = path.ToList();
        }

        public List<int> Target()
        {
            return path.ToList();
        }

        public Shrub<T> evaluate<T>(Shrub<T> Term)
        {
            (var debuijn, var arity) = Util.ParseCombinator(c)
                .Match(
                    pi => pi,
                    u => throw new Exception(u.ToString())
                );

            return Term.ApplyAt(
                t =>
                {
            
                    var args = t.Match(l => l,v => throw new Exception());
            
                    var result = Shrub<T>.Collapse(debuijn.Map(i => i + 1).Map(i => args[i]));
                
                    return result.Match(
                        l => Shrub<T>.Node(l.Concat(args.Skip(arity + 1)).ToList()), 
                        u => Shrub<T>.Node(new List<Shrub<T>>().Append(Shrub<T>.Leaf(u)).Concat(args.Skip(arity + 1)).ToList()));
                    
                },    
            path);
        }
    }
    
    
    public class Util
    {
        
        public static List<T> CanEvaluate<T>(Term term,List<int> path, Func<Term,ElimRule,T> cont)
        {
            return term.Match<List<T>>(
                children =>
                {
                    if (children.Count == 0)
                    {
                        return new List<T>();
                    }

                    List<T> EvalArgs = children
                        .Skip(1)
                        .SelectMany((shrub,index) => CanEvaluate<T>(shrub,path.Append(index + 1).ToList(), cont))
                        .ToList();
                    //depth first ordering of argument evaluation

                    return children[0].Match<List<T>>
                    (
                        LeftSidechildren =>
                        {
                            List<T> EvalLeftSide = CanEvaluate<T>(children[0],path.Append(0).ToList(), cont);

                            return EvalLeftSide
                                .Prepend(cont(term, new ParenElim(children, LeftSidechildren,path)))
                                .Concat(EvalArgs).ToList();
                        },
                        sum => sum.Match(
                            c =>
                            {
                                if (c.arity <= children.Count - 1)
                                {
                                    return EvalArgs.Prepend(cont(term, new CombinatorElim(c, children,path))).ToList();
                                }
                                else
                                {
                                    return EvalArgs;
                                }
                            }
                            ,
                            v => EvalArgs
                        )

                    );

                },
                value => new List<T>()
            );
        }
        
                public static Sum<Tuple<Shrub<int>,int>,Unit> ParseCombinator(Combinator c)
        {
            //Rules for writing combinator definitions
            // 1. Write variables as their debruijn index [x is 0, y is 1, z is 2 ...]
            // 2. There are no lambdas. If you need one, create a new combinator or
            // 3. To self reference, use a capital "Y" [the Y combinator can be written as "0(Y0)"]
            // 4. Use parenthesis to designate order of application as usual
            // 5. It most be non-empty
            // 6. Don't have more than ten variables
            
            //Examples
            // Bxyz => x(yz) | 0(12)
            // Txy => yx     | 10
            // Cxyz => xzy   | 021
            // Mx => xx      | 00
            // Yx => x(Yx)   | 0(~10)
            

            
            string input = c.lambdaTerm;
            

            
            if(input == null || input.Equals(""))
                return Sum<Tuple<Shrub<int>, int>, Unit>.Inr(new Unit());


            input = String.Concat(input.Where(ch => !char.IsWhiteSpace(ch)));
         
            char name = input[0];
            Dictionary<char,int> vars = new Dictionary<char,int>();

            int i = 1;
            while (!input[i].Equals('='))
            {
                if (input.Length <= i)
                {
                    Debug.Log($"input length = {input.Length}, i = {i}");
                    return Sum<Tuple<Shrub<int>,int>,Unit>.Inr(new Unit());
                }  else
                {
                    if (vars.ContainsKey(input[i]))
                    {
                        Debug.Log($"non-unique var name {input[i]}");
                        return Sum<Tuple<Shrub<int>, int>, Unit>.Inr(new Unit());
                    }
                    vars.Add(input[i],i - 1);
                    i++;
                }
            }
            vars.Add(name,-1); //recursion done with -1
            

            if (!input[++i].Equals('>'))
            {
                Debug.Log($"Missing \'>\' at index {i}, instead found {input[i]}");
                return Sum<Tuple<Shrub<int>, int>, Unit>.Inr(new Unit());
            }
            
            i++;

            (List<Shrub<char>> shrub,List<char> rest) =
                ParseParens(input.Skip(i).ToList(), ch => ch.Equals('('), ch => ch.Equals(')'));
            if (rest.Count != 0)
            {
                Debug.Log($"Parse parens failed. rest = {String.Concat(rest)}, shrub {Shrub<char>.Node(shrub)}");
                return Sum<Tuple<Shrub<int>, int>, Unit>.Inr(new Unit());
            }

            var output = Shrub<char>.Node(shrub).Map<int>(ch => vars[ch]);

            
            return Sum<Tuple<Shrub<int>, int>, Unit>.Inl(Tuple.Create<Shrub<int>, int>(output,vars.Count - 1));



        }

        public static Tuple<List<Shrub<T>>,List<T>> ParseParens<T>(List<T> l, Func<T,bool> is_left_paren, Func<T,bool> is_right_paren)
        {
            if (l.Count() != 0)
            {
                if(is_right_paren(l[0]))
                    return Tuple.Create(new List<Shrub<T>>(),l.Skip(1).ToList());
                if (is_left_paren(l[0]))
                {
                    (List<Shrub<T>> child,List<T> k) = ParseParens<T>(l.Skip(1).ToList(), is_left_paren, is_right_paren);
                    (List<Shrub<T>> parent, List<T> kk) = ParseParens<T>(k, is_left_paren, is_right_paren);
                    return Tuple.Create(parent.Prepend(Shrub<T>.Node(child)).ToList(),kk);
                }

                (List<Shrub<T>> toplevel,List<T> rest) = ParseParens<T>(l.Skip(1).ToList(), is_left_paren, is_right_paren);
                return Tuple.Create(toplevel.Prepend(Shrub<T>.Leaf(l[0])).ToList(),rest);
            }
            else
            {
                return Tuple.Create(new List<Shrub<T>>(),l);
            }
        }
    }
}