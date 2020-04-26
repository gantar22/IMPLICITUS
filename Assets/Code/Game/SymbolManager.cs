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
    private struct ForwardData
    {
        public ElimRule rule;
        public LayoutTracker TopSymbol;

        public ForwardData(ElimRule rule, LayoutTracker topSymbol)
        {
            this.rule = rule;
            TopSymbol = topSymbol;
        }
    }

    private struct BackwardTermData
    {
        public Term newTerm;
        public Func<LayoutTracker> lt;
        public Combinator C;
        public List<int> path;
        public LayoutTracker TopSymbol;

        public BackwardTermData(Term newTerm, Func<LayoutTracker> lt, Combinator c, List<int> path, LayoutTracker topSymbol)
        {
            this.newTerm = newTerm;
            this.lt = lt;
            C = c;
            this.path = path;
            TopSymbol = topSymbol;
        }
    }

    private struct BackwardParenData
    {
        public List<int> path;
        public int size;
        public Func<LayoutTracker> paren;
        public LayoutTracker TopSymbol;
        public Term newTerm;

        public BackwardParenData(List<int> path, int size, Func<LayoutTracker> paren, LayoutTracker topSymbol, Term newTerm)
        {
            this.path = path;
            this.size = size;
            this.paren = paren;
            TopSymbol = topSymbol;
            this.newTerm = newTerm;
        }
    }
    
    
    
    [SerializeField] public RectTransform skeletonRoot;
    [SerializeField] private RectTransform skeletonParen;
    [SerializeField] private RectTransform skeletonAtom;

    [SerializeField] private LayoutTracker parenSymbol;
	[SerializeField] private SpellList spellList;
    [SerializeField] private LayoutTracker[] Variables;

    [SerializeField] IntEvent effectAudioEvent; //Event Calls audio sound

    public List<Action<Term>> onCreateTerm;
    
    
    private Stack<ForwardData> forwardUndoStack = new Stack<ForwardData>();
    private Stack<Sum<BackwardTermData,BackwardParenData>> backwardUndoStack = new Stack<Sum<BackwardTermData, BackwardParenData>>();
    

    
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
        LayoutRebuilder.MarkLayoutForRebuild(parent);
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

    private static Transform AccessTransfrom(Transform t, List<int> path)
    {
        return AccessTransfrom(t, path, x => x,() => throw new IndexOutOfRangeException());
    }
    private static T AccessTransfrom<T>(Transform t,List<int> path, Func<Transform,T> k, Func<T> F)
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


    public IEnumerator popBackwards()
    {
        if (backwardUndoStack.Any())
        {
            yield return backwardUndoStack.Pop().Match(_ => _UnTransition(_.newTerm,_.lt(),_.C,_.path,_.TopSymbol),
                _ =>_BackApplyParens(_.path,_.size,_.paren(),_.TopSymbol,_.newTerm));
        }
        else
        {
            Debug.Log("no moves available");
        }
    }

    public bool HasBackStack()
    {
        return backwardUndoStack.Any();
    }

    public bool HasForStack()
    {
        return forwardUndoStack.Any();
    }

    public IEnumerator popForwards()
    {
        if (forwardUndoStack.Any())
        {
            var _ = forwardUndoStack.Pop();
            yield return (_Transition(_.rule, _.TopSymbol));
        }
        else
        {
            Debug.Log("no moves available");
        }
    }

    public IEnumerator Transition(ElimRule rule, LayoutTracker TopSymbol)
    {
        forwardUndoStack = new Stack<ForwardData>();

        yield return _Transition(rule, TopSymbol);
    }


    public void pushForward(ElimRule rule, LayoutTracker TopSymbol)
    {
        Term term = currentTerm;
        if (rule is CombinatorElim cElim)
        {
            var copy = Instantiate(AccessTransfrom(TopSymbol.transform, cElim.Target()).GetChild(0).gameObject,AccessTransfrom(TopSymbol.transform, cElim.Target()).GetChild(0).position,Quaternion.identity);
            copy.transform.SetParent(GetComponentInParent<Canvas>().transform.GetChild(0),true);
            copy.SetActive(false);
            backwardUndoStack.Push(Sum<BackwardTermData, BackwardParenData>.Inl(new BackwardTermData(term
                ,() => 
                {
                    copy.SetActive(true);
                    copy.transform.SetParent(AccessTransfrom(TopSymbol.transform, cElim.Target()));
                    copy.transform.localScale = Vector3.one;
                    return copy.GetComponent<LayoutTracker>();
                }
                ,cElim.c,cElim.Target(),TopSymbol
            )));
        } else if (rule is ParenElim pElim)
        {           
            var copy = Instantiate(AccessTransfrom(TopSymbol.transform, pElim.Target()).gameObject);
            copy.transform.SetParent(GetComponentInParent<Canvas>().transform.GetChild(0),true);
            copy.SetActive(false);
            int size = AccessTransfrom(TopSymbol.transform, pElim.Target()).childCount;
            backwardUndoStack.Push(Sum<BackwardTermData, BackwardParenData>.Inr(new BackwardParenData(
                pElim.Target(),size
                ,() => 
                {
                    copy.SetActive(true);
                    copy.transform.SetParent(AccessTransfrom(TopSymbol.transform, pElim.Target()));                    
                    copy.transform.localScale = Vector3.one;
                    return copy.GetComponent<LayoutTracker>();
                },TopSymbol,pElim.evaluate(term)
            )));
        }
    }
    void IterateTransform(Transform t,Action<Transform> f)
    {
        foreach (Transform t2 in t)
        {
            f(t2);
            IterateTransform(t2,f);
        }
    }


    public IEnumerator UnTransition(Term newTerm, LayoutTracker lt, Combinator C, List<int> path, LayoutTracker TopSymbol)
    {
        backwardUndoStack = new Stack<Sum<BackwardTermData, BackwardParenData>>();
        yield return _UnTransition(newTerm,lt,C,path,TopSymbol);
    }

    public IEnumerator BackApplyParens(List<int> path, int size, LayoutTracker paren, LayoutTracker TopSymbol, Term newTerm)
    {
        backwardUndoStack = new Stack<Sum<BackwardTermData, BackwardParenData>>();
        
        yield return _BackApplyParens(path,size,paren,TopSymbol,newTerm);
    }
    
    private IEnumerator _Transition( ElimRule rule, LayoutTracker TopSymbol) //no moving in topsymbol
    {
        pushForward(rule,TopSymbol);
        var oldTerm = currentTerm;
        /********* replace skeleton ***********/
        Term newTerm = rule.evaluate(oldTerm);
        currentTerm = newTerm;
        
        foreach (Transform t in skeletonRoot)
        {
            Destroy(t.gameObject);
        }
        CreateSkeleton(newTerm,skeletonRoot);
        
        
        
        /********* Find symbol where rule applies ***********/
        var index = rule.Target();
        var affectedSymbol = AccessTransfrom(TopSymbol.transform, index).GetComponent<LayoutTracker>();
        var leftmostchild = affectedSymbol.transform.GetChild(0).position;
        

        
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
                        var paren = Instantiate(parenSymbol,leftmostchild,Quaternion.identity,GetComponentInParent<Canvas>().transform);
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
                    t.GetComponent<LayoutTracker>().LockDown(.15f);

                }
                Destroy(dummyParen.gameObject);
                
                
                
                /****** adjust unused symbols indices ********/

                
                for (int i = arity + 1;false; i++)
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
            //TODO fade these out
            Destroy(old_paren.gameObject);
        }
        
        
        yield return new WaitUntil(() =>
        {
            bool moving = false;
            IterateTransform(TopSymbol.transform, t =>
            {
                if (t.GetComponent<LayoutTracker>())
                    moving |= t.GetComponent<LayoutTracker>().Moving();
            });
            return !moving;
        });

    }

    public IEnumerator _UnTransition(Term newTerm, LayoutTracker lt, Combinator C, List<int> path, LayoutTracker TopSymbol)
    {
        forwardUndoStack.Push(new ForwardData(new CombinatorElim(C,path), TopSymbol));
        /*
         * REMEMBER, you've already "typechecked" this operation, you can assume that everything fits
         * Get transform at path
         * Grab transforms at the first occurence of each debruijn index
         * Delete everything else
         * spawn metavariables
         * order everything appropriately
         */
        
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
        
        lt.LockDown(.1f);
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
        yield return new WaitUntil(() =>
        {
            bool moving = false;
            IterateTransform(TopSymbol.transform, t =>
            {
                if (t.GetComponent<LayoutTracker>())
                    moving |= t.GetComponent<LayoutTracker>().Moving();
            });
            return !moving;
        });
    }

    public IEnumerator _BackApplyParens(List<int> path, int size,LayoutTracker paren, LayoutTracker TopSymbol, Term newTerm)
    {
        forwardUndoStack.Push(new ForwardData(new ParenElim(path),TopSymbol));
        var target = AccessTransfrom(TopSymbol.transform, path);


        List<Transform> temp = new List<Transform>();
        for (int i = 0; i < size; i++)
            temp.Add(target.GetChild(i));


        paren.transform.parent = target; //this is the problem line. SetParent(target,true) doesn't work and is oddly less effective, lateupdate?
        
        paren.transform.SetAsFirstSibling();
        
        foreach (var t in temp)
        {
            t.SetParent(paren.transform,true);
        }



        foreach (Transform t in skeletonRoot)
        {
            Destroy(t.gameObject);
        }
        CreateSkeleton(newTerm,skeletonRoot);        
        yield return new WaitUntil(() =>
        {
            bool moving = false;
            IterateTransform(TopSymbol.transform, t =>
            {
                if (t.GetComponent<LayoutTracker>())
                    moving |= t.GetComponent<LayoutTracker>().Moving();
            });
            return !moving;
        });


    }
    
    

    //Figures out which Combinator Effect to play
    private void combinatorEffectPlay(CombinatorElim CElim)
    {
        //Check to ensure that effectAudioEvent is not null
        if (effectAudioEvent == null)
        {
            Debug.Log("UnitEvent \"effectAudioEvent\" missing from a script \"SymbolManager\"");
            return;
        }

        Debug.Log("Is Running Combinator Effect Play");

        //Getting name of combinator to reference
        char comb_name = CElim.c.info.nameInfo.name;

        //Figures out which sound to play for combinator families

        if (comb_name == 'B' || comb_name == 'D' || comb_name == 'E')
        {
            effectAudioEvent.Invoke(8); //B Combinator Sound
        }
        else if (comb_name == 'T' || comb_name == 'C' || comb_name == 'F' || comb_name == 'R' || comb_name == 'V')
        {
            effectAudioEvent.Invoke(9); //T Combinator Sound
        }
        else if (comb_name == 'Q')
        {
            effectAudioEvent.Invoke(10); //Q Combinator Sound
        }
        else if (comb_name == 'W' || comb_name == 'L')
        {
            effectAudioEvent.Invoke(11); //W Combinator Sound
        }
        else if (comb_name == 'K')
        {
            effectAudioEvent.Invoke(12); //K Combinator Sound
        }
        else if (comb_name == 'I')
        {
            effectAudioEvent.Invoke(13); //I Combinator Sound
        }
        else
        {
            //Combinator is not currently labeled, will play default button sound
            Debug.Log("Combinator with Name" + comb_name + "seems to be missing from list in" +
                      "script \"SymbolManager\" and function \"combinatorEffectPlay\"");

            effectAudioEvent.Invoke(0); //Button Press Sound
        }
    }
}
