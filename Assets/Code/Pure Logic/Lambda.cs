using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using UnityEngine;
using TypeUtil;
using UnityEditor;
using UnityEditor.PackageManager;
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


        public static BinaryTree<T> ToBinary<T>(Shrub<T> shrub)
        {
            return shrub.Match<BinaryTree<T>>(l =>
            {
                int n = l.Count;
                if (n == 0)
                {
                    throw new Exception("Invalid shrub, empty parenthesis");
                }

                if (n == 1)
                    return ToBinary<T>(l[0]);

                return BinaryTree<T>.Node(ToBinary<T>(Shrub<T>.Node(l.Take(n - 1).ToList())), ToBinary<T>(l[n - 1]));

            }, x => BinaryTree<T>.Leaf(x));
        }

        public static Shrub<T> FromBinary<T>(BinaryTree<T> tree)
        {
            return tree.Match<Shrub<T>>((l, r) =>
            {
                var (ll,rr) = (FromBinary<T>(l),FromBinary<T>(r));
                return ll.Match<Shrub<T>>(
                    ls => rr.Match(rs => Shrub<T>.Node(ls.Append(Shrub<T>.Node(rs)).ToList()),
                                      xr => Shrub<T>.Node(ls.Append(Shrub<T>.Leaf(xr)).ToList())),
                    xl => 
                        rr.Match(rs => Shrub<T>.Node(rs.Prepend(Shrub<T>.Leaf(xl)).ToList()),
                                 xr => Shrub<T>.Node(new List<Shrub<T>> {Shrub<T>.Leaf(xl),Shrub<T>.Leaf(xr)}) 
                                 ));
            }, Shrub<T>.Leaf);
        }
        
        public static Sum<Term, Unit> BackApply(Term term, Combinator C, List<int> path)
        {
            var target = term.Access(path);
            var (debruijn,arrity) = Lambda.Util.ParseCombinator(C).Match(p => p,_ => throw new ArgumentException());

           // var target = ToBinary(targetShrub);
           // var debruijn = ToBinary(debruijnShrub);

            Sum<Term,Unit> UnifyMeta(Term t1, Term t2)
            {
                return t1.Match<Sum<Term,Unit>>(l1 => t2.Match<Sum<Term,Unit>>(l2 =>
                    {
                        if(l1.Count != l2.Count)
                            return Sum<Term, Unit>.Inr(new Unit());
                        List<Term> result = new List<Term>();
                        for (int i = 0; i < l1.Count; i++)
                        {
                            if (UnifyMeta(l1[i], l2[i]).Match(t =>
                            {
                                result.Add(t);
                                return false;
                            }, _ => true))
                                return Sum<Term, Unit>.Inr(new Unit());
                        }

                        return Sum<Term, Unit>.Inl(Term.Node(result));
                    }
                , x2 => x2.Match(c2 => Sum<Term, Unit>.Inr(new Unit()), v2 =>
                {
                    if (((int) v2) == -1)
                    {
                        return Sum<Term, Unit>.Inl(t1);
                    }
                    else
                    {
                        return Sum<Term, Unit>.Inr(new Unit());
                    }
                })
            ), x1 => t2.Match<Sum<Term,Unit>>(l2 =>  x1.Match(c1 => Sum<Term, Unit>.Inr(new Unit()), v1 =>
                {
                    if (((int) v1) == -1)
                    {
                        return Sum<Term, Unit>.Inl(t2);
                    }
                    else
                    {
                        return Sum<Term, Unit>.Inr(new Unit());
                    }
                }), x2 =>
                    x1.Match<Sum<Term,Unit>>(
                         c1 => x2.Match<Sum<Term,Unit>>(
                             c2 => c1.Equals(c2) ? Sum<Term, Unit>.Inl(t1) : Sum<Term, Unit>.Inr(new Unit()),
                             v2 => (int)v2 == -1 ? Sum<Term, Unit>.Inl(t1) : Sum<Term, Unit>.Inr(new Unit()))
                        ,v1 => (int)v1 == -1 ? Sum<Term, Unit>.Inl(t2) : 
                             x2.Match<Sum<Term,Unit>>(
                             c2 => Sum<Term, Unit>.Inr(new Unit()),
                             v2 => (int)v2 == -1 ? Sum<Term, Unit>.Inl(t1) : 
                                 (v1 == v2 ? Sum<Term, Unit>.Inl(t1) : Sum<Term, Unit>.Inr(new Unit())) ))
                ));
            }
            
            bool UnifyDebruijn(Shrub<int> d, Term t, Term[] subst)
            {
                //true if unification works
                return d.Match<bool>(
             ds => t.Match<bool>(ts =>
             {
                 if (ds.Count != ts.Count)
                     return false;
                 for (int i = 0; i < ds.Count; i++)
                 {
                     if (!UnifyDebruijn(ds[i], ts[i], subst))
                         return false;
                 }
    
                 return true;
             }
                , xt => { return xt.Match<bool>(c => false, v => (int)v == -1); }), //unification with metavariable unnecesary
            xd => t.Match<bool>(ts =>
            {
                return UnifyMeta(subst[xd], t).Match(
                    unified =>
                    {
                        subst[xd] = unified;
                        return true;
                    }, _ => false); //can't apply duplicator
            }, xt =>
            {
                return xt.Match<bool>(c =>
                    UnifyMeta(subst[xd],
                        Term.Leaf(Sum<Combinator, Variable>.Inl(c))).Match(
                        unified =>
                        {
                            subst[xd] = unified;
                            return true;
                        }, _ => false) //can't apply duplicator
                        , v =>
                        {
                            if ((int) v == -1)
                            {
                                return true;
                            }
                            else
                            {
                                return UnifyMeta(subst[xd],
                                    Term.Leaf(
                                        Sum<Combinator, Variable>.Inr(v))).Match(
                                    unified =>
                                    {
                                        subst[xd] = unified;
                                        return true;
                                    }, _ => false);
                                //unify
                            }
                        }
                    );
            }));
            }

            Term[] sub = new Term[arrity];
            if (UnifyDebruijn(debruijn, target, sub))
                return Sum<Term,Unit>.Inl(term.Update(Term.Node(sub.ToList()), path));
            return Sum<Term, Unit>.Inr(new Unit());
            
            //binarify everything TODO don't do this or return a List<Term>
            //unify -- negative variables are metavariables (can be substituted by anything)
            //if unification fails retry with degenerate arity extensions until we hit the left depth of target
            //apply the unification to debruijn adding metavariablese
            // \xyz => (y z) backapplied to ([X]) (where [X] is a metavariable) becomes [Y] [X_1] [X_2]
            // [Y] is created because of the dropped argument and [X_i] gets created as we unify for [X] ~ (y z)
            //unification returns an array of shrubs where each cell holds what needs to be subst in (initialized to new metavariables), and deals with replaces metavariables in the target which might be bad

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