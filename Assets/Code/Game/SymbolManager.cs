using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Lambda;
using UnityEngine;
using TypeUtil;
using UnityEngine.Events;
using UnityEngine.Experimental.TerrainAPI;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator,Lambda.Variable>>;
using UnityEngine.UI;

[RequireComponent(typeof(TermClickHandler))]
public class SymbolManager : MonoBehaviour
{
    [SerializeField] public RectTransform skeletonRoot;
    [SerializeField] private RectTransform skeletonParen;
    [SerializeField] private RectTransform skeletonAtom;

    [SerializeField] private LayoutTracker parenSymbol;
	[SerializeField] private SpellList spellList;
    [SerializeField] private LayoutTracker[] Variables;

    [SerializeField] IntEvent effectAudioEvent; //Event Calls audio sound

    public List<Action<Term>> onCreateTerm;

    
    private Term currentTerm;

    public void Awake()
    {
        onCreateTerm = new List<Action<Term>>();
    }

    private LayoutTracker GetSymbol(Sum<Combinator,Lambda.Variable> v)
    {
        //TODO use dictionary
        var lt = v.Match(c => spellList.spells.First(s => s.combinator.Equals(c)).prefab, x => Variables[0]);
        v.Match(c => {}, x => lt.GetComponent<SetVariablePrefab>().Set((int)x));
        return lt;
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

        foreach (Transform t in skeletonRoot)
        {
            if(Application.isPlaying)
                Destroy(t.gameObject);
        }
        CreateSkeleton(currentTerm.Insert(path, x),skeletonRoot);
    }

    public void HandleClick(List<int> path, LayoutTracker root)
    {
        GetComponent<TermClickHandler>()?.HandleClick(this,currentTerm, path,root);
    }
    
    public void Append(List<Term> x)
    {
        
        foreach (Transform t in skeletonRoot)
        {
            if(Application.isPlaying)
                Destroy(t.gameObject);
        }
        CreateSkeleton(currentTerm.Match(
            l => Term.Node(l.Concat(x).ToList()),
            _ => throw new Exception("current term not application")),skeletonRoot);
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
        if (parent == skeletonRoot)
            onCreateTerm?.ForEach(f => f?.Invoke(term)); //oncreateterm is null? something something serializable?
    }

    private LayoutTracker CreateSymbols(Term term, Transform parent, List<int> path, int index)
    {
        return term.Match<LayoutTracker>(l =>
        {
            LayoutTracker symbol = Instantiate(parenSymbol,parent);
            symbol.root = skeletonRoot;
            symbol.index = path.Append(index).ToList();
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
            for (var i = 0; i < l.Count; i++)
            {
                CreateSymbols(l[i], symbol.transform, path.Append(index).ToList(), i);
            }
            
            return symbol;
        }, v =>
        {
            var symbol = Instantiate(GetSymbol(v),parent);
            
            symbol.index = path.Append(index).ToList();
            symbol.root = skeletonRoot;
            return symbol;
        });
    }

    public LayoutTracker RemoveAtAndReturn(List<int> path, LayoutTracker root)
    {
        var index = path[path.Count - 1];
        path = path.Take(path.Count - 1).ToList();
        var new_term = currentTerm.ApplyAt(parenTerm => parenTerm.Match(l =>
            {
            l = l.ToList();
            var RemovedTerm = l[index];
            l.RemoveAt(index);
            RemovedTerm.Match(children =>
            {
                children.Reverse();
                foreach (Term child in children)
                {
                    l.Insert(index,child);
                }
            }, _ => { /* already removed */ });
            return Term.Node(l);
            }
           
            ,x => throw new Exception("Depth Exception")) , path);
        
        foreach (Transform t in skeletonRoot)
        {
            if(Application.isPlaying)
                Destroy(t.gameObject);
        }
        CreateSkeleton(new_term,skeletonRoot); //sets new term
        var Paren = AccessTransfrom(root.transform, path);
        Paren.name = "shitboy";
        Debug.Log(Paren);
        
        Debug.Log($"index {index}");
        var Removed = Paren.GetChild(index);
        
       
        List<Transform> ts = new List<Transform>();
        foreach (Transform t in Removed.transform)
            ts.Add(t);

        ts.Reverse();
        foreach (Transform t in ts)
        {
            t.SetParent(Paren);
            t.SetSiblingIndex(index);
        }
        return Removed.GetComponent<LayoutTracker>();
    }

    private Transform AccessTransfrom(Transform t, List<int> path)
    {
        return AccessTransfrom(t, path, x => x,() => throw new IndexOutOfRangeException());
    }
    private T AccessTransfrom<T>(Transform t,List<int> path, Func<Transform,T> k, Func<T> F)
    {
        foreach (int i in path)
        {
            if (t.childCount <= i)
            {
                return F();
            }
            t = t.GetChild(i);
        }

        return k(t);
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

            //Plays the appropriate combinator sound effect
            combinatorEffectPlay(CElim); 

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
                        symbols[ind + 1].transform.SetParent(GetComponentInParent<Canvas>().transform,true);
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
                        var paren = Instantiate(parenSymbol.gameObject,GetComponentInParent<Canvas>().transform).GetComponent<LayoutTracker>();
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

    public void UnTransition(Term newTerm, LayoutTracker lt, Combinator C, List<int> path, LayoutTracker TopSymbol)
    {
        /*
         * REMEMBER, you've already "typechecked" this operation, you can assume that everything fits
         * Get transform at path
         * Grab transforms at the first occurence of each debruijn index
         * Delete everything else
         * spawn metavariables
         * order everything appropriately
         */
        var oldTerm = currentTerm;
        
        Transform target = AccessTransfrom(TopSymbol.transform, path);
        (var debruijn, var arity) = Util.ParseCombinator(C)
            .Match(
                pi => pi,
                u => throw new Exception(u.ToString())
            );
        
        debruijn.Match(d =>
        {
            d = d.ToList();
            for (int i = d.Count; i < target.childCount; i++)
            {
                d.Add(Shrub<int>.Leaf(arity));
                arity++;
            }

            debruijn = Shrub<int>.Node(d);
        }, x =>
        {
            var d = new List<Shrub<int>>();
            d.Add(Shrub<int>.Leaf(x));
            for (int i = d.Count; i < target.childCount; i++)
            {
                d.Add(Shrub<int>.Leaf(arity));
                arity++;
            }

            debruijn = Shrub<int>.Node(d);
                    
        });
        
        
        Transform[] children = new Transform[arity];
        debruijn.IterateI(new List<int>(), (i, p) =>
        {
            children[i] = AccessTransfrom(target, p,t => t,() => null);
        });

        var canvas = GetComponentInParent<Canvas>();
        foreach (var child in children)
        {
            child?.SetParent(canvas.transform,true);
        }


        foreach (Transform t in target)
        {
            Destroy(t.gameObject);
        }
        
        lt.transform.SetParent(target,true);

        foreach (var child in children)
        {
            if (!child)
                Instantiate(GetSymbol(Sum<Combinator, Variable>.Inr((Variable) (-1))),target);
            else
                child.SetParent(target,true);
        }

        foreach (Transform t in skeletonRoot)
        {
            Destroy(t.gameObject);
        }
        CreateSkeleton(newTerm,skeletonRoot);
    }

    public void backApplyParens(List<int> path, int size,LayoutTracker paren, LayoutTracker TopSymbol, Term newTerm)
    {
        var target = AccessTransfrom(TopSymbol.transform, path);


        List<Transform> temp = new List<Transform>();
        for (int i = 0; i < size; i++)
            temp.Add(target.GetChild(i));

        foreach (var t in temp)
        {
            t.SetParent(paren.transform,true);
        }
        
        paren.transform.SetParent(target,true);
        paren.transform.SetSiblingIndex(0);
        
        
        foreach (Transform t in skeletonRoot)
        {
            Destroy(t.gameObject);
        }
        CreateSkeleton(newTerm,skeletonRoot);
    }

    //Figures out which Combinator Effect to play
    private void combinatorEffectPlay(CombinatorElim CElim)
    {
        Debug.Log("Is Running Combinator Effect Play");
        //if Combinator "B"
        //effectAudioEvent.Invoke(8); //B Combinator Sound

        //if Combinator "T"
        //effectAudioEvent.Invoke(9); //T Combinator Sound

        //if Combinator "Q"
        //effectAudioEvent.Invoke(10); //Q Combinator Sound

        //if Combinator "W"
        //effectAudioEvent.Invoke(11); //W Combinator Sound

        //if Combinator "K"
        //effectAudioEvent.Invoke(12); //K Combinator Sound

        //if Combinator "I"
        //effectAudioEvent.Invoke(13); //I Combinator Sound

    }

}
