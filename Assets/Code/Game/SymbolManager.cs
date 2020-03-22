using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lambda;
using UnityEngine;
using TypeUtil;
using UnityEngine.Experimental.TerrainAPI;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator,Lambda.Variable>>;

public class SymbolManager : MonoBehaviour
{
    [SerializeField] private RectTransform skeletonRoot;
    [SerializeField] private RectTransform skeletonParen;
    [SerializeField] private RectTransform skeletonAtom;

    [SerializeField] private LayoutTracker parenSymbol;
    [SerializeField] private Spell[] spells;
    [SerializeField] private LayoutTracker[] Variables;

    private Term currentTerm;


    private LayoutTracker GetSymbol(Sum<Combinator,Lambda.Variable> v)
    {
        //TODO use dictionary
        return v.Match(c => spells.First(s => s.combinator.Equals(c)).prefab, x => Variables[(int) x]);
    }
    
    public LayoutTracker Initialize(Term term)
    {
        foreach (Transform t in skeletonRoot)
        {
            if(Application.isPlaying)
                Destroy(t.gameObject);
        }
        CreateSkeleton(term,skeletonRoot);
        return CreateSymbols(term, transform, new List<int>(), 0);
    }


    public Term readTerm()
    {
        return currentTerm;
    }

    public void Insert(List<int> path, Term x)
    {
        foreach (int i in path)
        {
            print($"path contains: {i}");
        }

        foreach (Transform t in skeletonRoot)
        {
            if(Application.isPlaying)
                Destroy(t.gameObject);
        }
        print($"term = {currentTerm}");
        print($"inserted term = {currentTerm.Insert(path,x)}");
        CreateSkeleton(currentTerm.Insert(path, x),skeletonRoot);
    }

    
    
    private void CreateSkeleton(Term term, RectTransform parent)
    {
        term.Match(l =>
        {

            RectTransform newSkeleton = Instantiate(skeletonParen, parent);
            foreach (var shrub in l)
            {
                CreateSkeleton(shrub, newSkeleton);
            }
        }, v => Instantiate(skeletonAtom, parent));
        
        currentTerm = term;
    }

    private LayoutTracker CreateSymbols(Term term, Transform parent, List<int> path, int index)
    {
        return term.Match<LayoutTracker>(l =>
        {
            LayoutTracker symbol = Instantiate(parenSymbol,parent);
            symbol.root = skeletonRoot;
            symbol.index = path.Append(index).ToList();
            for (var i = 0; i < l.Count; i++)
            {
                CreateSymbols(l[i], symbol.transform, path.Append(index).ToList(), i);
            }

            return symbol;
        }, v =>
        {
            var symbol = Instantiate(GetSymbol(v),parent);
            v.Match<Unit>(c => new Unit(), x =>
            {
                symbol.GetComponent<SetVariablePrefab>().Set((int)x);
                return new Unit();
            });
            symbol.index = path.Append(index).ToList();
            symbol.root = skeletonRoot;
            return symbol;
        });
    }

    private Transform AccessTransfrom(Transform t,List<int> path)
    {
        foreach (int i in path)
        {
            t = t.GetChild(i);
        }

        return t;
    }


    public LayoutTracker Transition(Term oldTerm, ElimRule rule, LayoutTracker TopSymbol)
    {
        /********* replace skeleton ***********/
        Term newTerm = rule.evaluate(oldTerm);

        foreach (Transform t in skeletonRoot)
        {
            Destroy(t.gameObject);
        }
        CreateSkeleton(newTerm,skeletonRoot);
        
        
        
        /********* Find symbol where rule applies ***********/
        var index = rule.Target();
        var affectedSymbol = AccessTransfrom(TopSymbol.transform, index).GetComponent<LayoutTracker>();
        
        /********* case on the rule **********/
        if (rule is CombinatorElim CElim)
        {

            /********** unpack the elim rule ***********/
            {
                
                (var debruijn, var arity) = Util.ParseCombinator(CElim.c)
                    .Match(
                        pi => pi,
                        u => throw new Exception(u.ToString())
                    );
                //keep track of the arguments that we need to destroy or whether
                //they are the first use of an arg so we don't need to spawn a new one
                bool[] argumentsThatHaveBeenUsed = new bool[arity + 1]; /* + 1 for possible recursion at A[0]*/
                
                //get all the symbols at the affected level in a list for easier management
                List<LayoutTracker> symbols = new List<LayoutTracker>();
                foreach (Transform t in affectedSymbol.transform)
                {
                    symbols.Add(t.GetComponent<LayoutTracker>());
                }

                /**************** map over transforms to adjust indices ***************/
                void IterateTransform(Transform t,Action<Transform> f)
                {
                    foreach (Transform t2 in t)
                    {
                        f(t2);
                        IterateTransform(t2,f);
                    }
                }
                
                /******* find the new symbols to replace the arguments with   ***********/
                var detatchedSymbols = debruijn.MapI(index.Append(0).ToList(), (ind, path) =>
                {
                    if (!argumentsThatHaveBeenUsed[ind + 1 /* +1 for recursion*/])
                    {
                        symbols[ind + 1].transform.SetParent(null,true);
                        symbols[ind + 1].index = path;
                        
                        IterateTransform(symbols[ind + 1].transform,
                            t => t.GetComponent<LayoutTracker>().index = path.Concat(t.GetComponent<LayoutTracker>().index.Skip(index.Count + 2)).ToList());
                         argumentsThatHaveBeenUsed[ind + 1] = true;
                        return symbols[ind + 1];
                    }
                    else
                    {
                       IterateTransform(symbols[ind+1].transform,
                            t => t.GetComponent<LayoutTracker>().index = path.Concat(t.GetComponent<LayoutTracker>().index.Skip(index.Count + 2)).ToList());
                        var newSymbol = Instantiate(symbols[ind + 1]);
                        newSymbol.index = path;
                        return newSymbol;
                    }
                });
                
                /************** Cleanup unused argument symbols **************/
                for (var i = 0; i < argumentsThatHaveBeenUsed.Length; i++)
                {
                    if (!argumentsThatHaveBeenUsed[i])
                    {
                        Destroy(symbols[i].gameObject);
                    }
                }

                /*******  Create Paren objects and set up hierarchy *********/

                Transform CreateParens(Shrub<LayoutTracker> symbolShrub,List<int> totalPath)
                {
                    return symbolShrub.Match<Transform>(l =>
                    {
                        var paren = Instantiate(parenSymbol);
                        paren.index = totalPath;
                        paren.root = skeletonRoot;
                        for (var i = 0; i < l.Count; i++)
                        {
                            CreateParens(l[i], totalPath.Append(i).ToList()).SetParent(paren.transform,true);
                        }

                        return paren.transform;
                    }, 
                        sym => sym.transform);
                }
                
                //create the paren object on a dummy object then move the new terms
                //to the appropriate place in the appropriate order
                Transform dummyParen = CreateParens(detatchedSymbols, index.Append(0).ToList()); //TODO verify
                
                List<Transform> ts = new List<Transform>();
                for (int i = dummyParen.childCount - 1; 0 <= i; i--)
                {
                    ts.Add(dummyParen.GetChild(i));
                }
                
                foreach(var t in ts)
                {
                    t.SetParent(affectedSymbol.transform,true);
                    t.SetSiblingIndex(0);
                }
                Destroy(dummyParen.gameObject);
                
                
                
                /****** adjust unused symbols indices ********/

                
                for (int i = arity + 1; i < symbols.Count; i++)
                {
                    int new_pos = i - arity - 1 + detatchedSymbols.Match(l => l.Count, v => 1);
                    symbols[i].index[symbols[i].index.Count - 1] = new_pos;
                    
                    IterateTransform(symbols[i].transform,t => t.GetComponent<LayoutTracker>().index[index.Count - 1] = new_pos); //TODO verify
                }
            }
            
        }

        if (rule is ParenElim PElim)
        {
            affectedSymbol.gameObject.name = "affected boi";

            var old_paren = affectedSymbol.transform.GetChild(0);
            var children = new List<Transform>();
            for (int i = old_paren.childCount - 1; i >= 0; i--)
            {  
                children.Add(old_paren.GetChild(i));
            }

            for(int child = 0; child < children.Count;child++)
            {
                LayoutTracker lt = children[child].GetComponent<LayoutTracker>();
                lt.index.RemoveAt(lt.index.Count - 1);
                lt.index[lt.index.Count - 1] += child;
                children[child].SetParent(affectedSymbol.transform,true);
                children[child].SetSiblingIndex(0); 
            }

            Destroy(old_paren.gameObject);
        }

        return null;
    }
    
    /*
     * Steps to get transistions working
     * 1. replace the skeleton
     * 2. Take in a rule and case on it
     * 3. Find the symbol that correspond to term where the rule applies
     * 3. find new locations for each symbol at that level
     * 4. duplicate or delete symbols to match new quantity
     * 5. set the symbols indicies
     */

}
