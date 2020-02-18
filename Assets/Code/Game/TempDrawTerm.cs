using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Lambda;
using TypeUtil;
using UnityEngine;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator,Lambda.Variable>>;
using Path = System.Collections.Generic.List<int>;



public class TempDrawTerm : MonoBehaviour
{
    [SerializeField] private List<Spell> spells;

    [SerializeField] private List<GameObject> variables;


    private Dictionary<Sum<Combinator,Lambda.Variable>, GameObject> lookup_object;

    private Dictionary<Sum<Combinator, Lambda.Variable>, float> lookup_width;

    Shrub<List<Path>> NewLocations(Term term, Lambda.ElimRule r)
    {
        if (r is CombinatorElim CElim)
        {
            (var debuijn, var arity) = Util.ParseCombinator(CElim.c)
                .Match(
                    pi => pi,
                    u => throw new Exception(u.ToString())
                );
            
            return term.MapI<List<Path>>(new List<int>(),((val, path) =>
            {
                if (path.Count == 0)
                {
                    //the whole term is just a val
                    return new List<Path>(); // TODO
                }

                if (path[0] > arity)
                {
                    //after the arguments and function
                    return new List<Path>();
                }
                else
                {
                    List<List<int>> benign_mutation = new List<List<int>>();
                    debuijn.IterateI(new Path(), (ind, p) =>
                    {
                        if (ind == path[0] - 1)
                        {
                            //we belong here
                            benign_mutation.Add(p);
                        }
                    });
                    return benign_mutation;

                }
            }));
        }

        if (r is ParenElim PElim)
        {
            
            int lhs_size = term.Match(l => l.Count, v => 1);
            return term.MapI<List<Path>>(new Path(), (val, path) =>
            {
                if (path.Count == 0)
                {
                    return new List<List<int>>();
                }

                if (path[0] == 0)
                {
                    return new List<Path>().Append(path.Skip(0).ToList()).ToList();
                }

                if (path[0] > 0)
                {
                    return new List<Path>().Append(path.Skip(0).Prepend(path[0] + lhs_size).ToList()).ToList();
                }
                return new List<Path>();
            } );
        }
        return new Shrub<List<Path>>();
    }
    
    
    Tuple<Shrub<float>,float> findLeftSides(Term term,float leftOffset)
    {
        return term.Match<Tuple<Shrub<float>,float>>(l =>
        {
            List<Shrub<float>> acc = new List<Shrub<float>>();
            float widths = 0;
            foreach (var shrub in l)
            {
                (var s, float width) = findLeftSides(shrub, leftOffset + widths);
                widths += width;
                acc.Add(s);
            }

            return Tuple.Create(Shrub<float>.Node(acc),widths);
        }, v =>  Tuple.Create(Shrub<float>.Leaf(leftOffset),lookup_width[v]));
    }

    Shrub<GameObject> SpawnTerm(Term term)
    {
        (var leftsides, var width) = findLeftSides(term, 0);
        leftsides = leftsides.Map(v => v + width / 2);
        return term.MapI(new Path(),((val, p) =>
        {
            return leftsides.Access(p).Match(l => null, 
                f => GameObject.Instantiate(lookup_object[val], f * Vector3.right,Quaternion.identity));
        }) );
    }

    Shrub<GameObject> TransferTerm(Term term, Shrub<GameObject> objs, ElimRule rule)
    {
        var paths_shrub = NewLocations(term, rule);
        var new_term = rule.evaluate(term);
        var new_leftsides = findLeftSides(new_term, 0);
        
        GameObject lookup(Path p)
        {
            GameObject go = null;
            paths_shrub.IterateI(new Path(), (List<List<int>> targets, List<int> source) =>
            {
                if (targets.Exists(target => target.SequenceEqual(p)))
                {
                    if (targets.FindIndex(target => target.SequenceEqual(p)) == 0)
                    {
                        objs.Access(source);
                    }
                    else
                    {
                        go = GameObject.Instantiate(objs.Access(source).Match(l => null,r => r));
                    }
                }
            });
            return go;
        }

        return new_term.MapI<GameObject>(new Path(), (_, path) =>
        {
            GameObject g = lookup(path);
            StartCoroutine(MoveTo(g, Vector3.right * new_leftsides.Item1.Access(path).Match(l => -0, r => r)));
            return g;
        });
    }

    IEnumerator MoveTo(GameObject g, Vector3 dest)
    {
        Vector3 vel = Vector3.zero;
        while ((g.transform.position - dest).magnitude > .025f)
        {
            yield return null;
            g.transform.position = Vector3.SmoothDamp(g.transform.position,dest,ref vel,.05f);
        }
    }
}
