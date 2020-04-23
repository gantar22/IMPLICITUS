using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lambda;
using TypeUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PreviewInfo = TypeUtil.Sum<DraggableSpell.PreviewForwardInfo,DraggableSpell.PreviewBackCombinatorInfo,DraggableSpell.PreviewRedundantParenInfo>;


[RequireComponent(typeof(CodexOnSpell))]
public class DraggableSpell : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    private Vector2 offset = Vector2.right;
    private RectTransform rt;
    private GraphicRaycaster gr;
    private EventSystem es;
    private Image duplicate;
    public Combinator myCombinator;
    public UnitEvent onApply;
    public UnitEvent onUnapply;
	public TermEvent pushUndoGoalTerm;
	public TermEvent pushUndoProposalTerm;
	private DraggableHolder.DraggableType myDraggableType;
    private bool evaluationMode = false;
	[HideInInspector]
    public bool hasBeenDragged = false;
    public BoolRef NoForwardMode;
    private const bool oneTry = true;

	private CodexOnSpell codexOnSpell;
    private Sum<PreviewInfo,Unit> previewState = Sum<PreviewInfo,Unit>.Inr(new Unit());
    
    public struct PreviewRedundantParenInfo
    {// keep as long as your index is the same
        public List<int> path;
        public int my_index;
        public SpawnTarget spawnTarget;
        public Action place;
        public Action unplace;

        public PreviewRedundantParenInfo(List<int> path, int myIndex, SpawnTarget spawnTarget, Action place, Action unplace)
        {
            this.path = path;
            my_index = myIndex;
            this.spawnTarget = spawnTarget;
            this.place = place;
            this.unplace = unplace;
        }
    }

    public struct PreviewBackCombinatorInfo
    {   //keep as long as paren is the same
        //highlights good or highlights bad or maybe just highlights good
        public SpawnTarget spawnTarget;
        public LayoutTracker paren;
        private List<int> path;
        public Action place;

        public PreviewBackCombinatorInfo(SpawnTarget spawnTarget, LayoutTracker paren, List<int> path, Action place)
        {
            this.spawnTarget = spawnTarget;
            this.paren = paren;
            this.path = path;
            this.place = place;
        }
    }

    public struct PreviewForwardInfo
    { //keep as long as path stays and index stays or goes up one
        public SymbolManager sm;
        public List<int> path;
        public Transform target;
        public int my_index;
        public Action place;
        public Action unplace;

        public PreviewForwardInfo(SymbolManager sm, List<int> path, Transform target, int myIndex, Action place, Action unplace)
        {
            this.sm = sm;
            this.path = path;
            this.target = target;
            my_index = myIndex;
            this.place = place;
            this.unplace = unplace;
        }
    }
    

	// Init
	private void Awake() {
		codexOnSpell = GetComponent<CodexOnSpell>();
	}

	private void Start()
    {
        DraggableHolder dh = GetComponentInParent<DraggableHolder>();
        
        if (dh)
            myDraggableType = GetComponentInParent<DraggableHolder>().myType;
        else
            myDraggableType = DraggableHolder.DraggableType.NoDragging;
        
        rt = GetComponent<RectTransform>();
        gr = GetComponentInParent<GraphicRaycaster>();
        es = GetComponentInParent<EventSystem>();

        if (myDraggableType != DraggableHolder.DraggableType.RedundantParens)
        {
            onApply.AddRemovableListener(_ => { enabled = false; }, this);
            onUnapply.AddRemovableListener(_ => { enabled = true; }, this);
        }

        onApply.AddRemovableListener(_ => evaluationMode = true,this);
        onUnapply.AddRemovableListener(_ => evaluationMode = false, this);
    }

    private bool NoDragging()
    {
        switch (myDraggableType)
        {
            case DraggableHolder.DraggableType.NoDragging:
            case DraggableHolder.DraggableType.RedundantParens when myCombinator != null:
            case DraggableHolder.DraggableType.RedundantParens when transform.GetSiblingIndex() != 0:
                return true;
            default:
                return false;
        }
    }
    
    public void OnDrag(PointerEventData eventData) {
		if (!hasBeenDragged)
            return;
        
        
        rt.position = Camera.main.ScreenToWorldPoint(Vector3.forward * 10 + Input.mousePosition);

        previewState.Match(
            //should I stop?
            sum => sum.Match(forward => {/*within one*/  }, backward => {/*paren changed*/ }, redundant =>
            {
                void Fail()
                {
                    redundant.unplace();
                    previewState = Sum<PreviewInfo, Unit>.Inr(new Unit());
                }

                (var spawnTarget, var targets) = getTargets();
                /* path has chaned or index != 0*/
                FreshPreview(spawnTarget,targets).Match(preview => preview.Match(_ => {/* fail*/}, _ => {/*fail*/}, state =>
                {
                    if (state.path.Count == redundant.path.Count && state.path.Select((x, i) => x == redundant.path[i]).All(x => x))
                    {
                        if (state.my_index == 0)
                        {
                            /* do nothing */
                        }
                        else
                        {
                            Fail();
                        }
                    } else if (state.path.Count == redundant.path.Count + 1 &&
                        redundant.path.Select((x, i) => state.path[i] == x).All(x => x))
                    {
                        if (state.my_index == targets[0].childCount)
                        {
                            /* do nothing */
                        }
                        else
                        {
                            Fail();
                        }
                    } else
                    {
                        Fail();
                    }
                }), _ =>
                {
                    Fail();
                });
            })
            , __ =>
        {
            //Should I start?
            var (spawnTarget, targets) = getTargets();
            previewState = FreshPreview(spawnTarget,targets);
            previewState.Match(sum => sum.Match(forward =>{/* place*/} , backward => {/* highlight*/}, redundant => redundant.place()),_ => { /* do nothing */ });
        });


    }

    public Sum<PreviewInfo,Unit> FreshPreview(SpawnTarget spawnTarget, List<Transform> targets)
    {
        
        if (targets.Count > 0)
        {
            return MakeDrop(spawnTarget, targets[0]).Match(_ =>  Sum<PreviewInfo, Unit>.Inr(new Unit()), sum =>
                Sum<PreviewInfo, Unit>.Inl(sum));
        }
        else
        {
            return Sum<PreviewInfo, Unit>.Inr(new Unit());   
        }
    }
    
    
    public void OnBeginDrag(PointerEventData eventData) {
		if (NoDragging())
            return;
        
        //create duplicate and do some sibling index stuff and layout element stuff
        if (myDraggableType == DraggableHolder.DraggableType.LeftPane)
        {
			codexOnSpell.deleteCodexTab();
            duplicate = Instantiate(gameObject,transform.parent).GetComponent<Image>();
            duplicate.transform.SetSiblingIndex(transform.GetSiblingIndex());
            duplicate.enabled = false;
            GetComponent<LayoutElement>().ignoreLayout = true;
        }
        else if(myDraggableType == DraggableHolder.DraggableType.Proposal || myDraggableType == DraggableHolder.DraggableType.RedundantParens)
        {
            UnPlace();
        }
        
        offset = transform.position;
		hasBeenDragged = true;
	}

    void UnPlace()
    {
        if (duplicate) duplicate.enabled = true;
        List<int> index = GetComponent<LayoutTracker>().index;
        SymbolManager sm = GetComponentInParent<SymbolManager>();
        var lt = sm.RemoveAtAndReturn(index.Skip(1).ToList(),sm.GetComponentInChildren<LayoutTracker>());
        lt.transform.SetParent(GetComponentInParent<Canvas>().transform,true);
        lt.transform.SetSiblingIndex(GetComponentInParent<Canvas>().transform.childCount - 1);
        lt.enabled = false;
    }
    
	// WARNING - this seems to not be getting called according to IEndDragHandler because
	//   CodexOnSpell implements IPointerClickHandler, and when they're both on the same GameObject,
	//   it will only call one of them. So instead, OnPointerClick() in CodexOnSpell will call this.
	//   (If there's an issue with this tho, let Dom know)
    public void OnEndDrag(PointerEventData eventData)
    {
        Drop();
    }
    
    
    public void Drop()
    {
    if (!hasBeenDragged)
            return;
        hasBeenDragged = false;

        
        (var spawnTarget,var L) = getTargets();
        
        if (L.Count == 0)
        {
            print("no hits");
            DestroyMe();
        }
        else
        {
            /*      Insert yourself into the term       */
            if (oneTry)
            {
                MakeDrop(spawnTarget, L[0]).Match(f => f(), s => s.Match(x => x.place(),x => x.place(),x => previewState = Sum<PreviewInfo, Unit>.Inr(new Unit())));
                return;
            }

            Action endAction = null;
            foreach (var target in L)
            {
                if (MakeDrop(spawnTarget, target).Match(failure =>
                {
                    endAction = failure;
                    return false;
                }, succ =>
                {
                    succ.Match(x => x.place(),x => x.place(),x => x.place());
                    return true;
                }))
                    return;
            }
            endAction?.Invoke();
        }
    
    }
    
    void DestroyMe()
    {
        if(myDraggableType == DraggableHolder.DraggableType.LeftPane)    
            duplicate.enabled = true;
        Destroy(gameObject);
    }
    
    void PlaceMe()
    {
        if(myDraggableType == DraggableHolder.DraggableType.LeftPane)    
            duplicate.enabled = true;
            
        var lt = GetComponent<LayoutTracker>();
        lt.enabled = true;
        lt.root = lt.GetComponentInParent<SymbolManager>().skeletonRoot;
    }

    private Sum<Action,PreviewInfo> MakeDrop(SpawnTarget spawnTarget, Transform target) //left is on failure, right is on success
    {
        var parenTracker = target.GetComponent<LayoutTracker>();
        
        List<int> index = parenTracker.index;
    
        var my_term = myCombinator == null
            ? Shrub<Sum<Combinator, Variable>>.Node(new List<Shrub<Sum<Combinator, Variable>>>())
            : Shrub<Sum<Combinator, Variable>>.Leaf(Sum<Combinator, Variable>.Inl(myCombinator));
        if (!target.GetComponentInParent<DraggableHolder>())
        {
            return Sum<Action, PreviewInfo>.Inl(DestroyMe);
        }
        
        if(spawnTarget && !spawnTarget.NoBackApplication.val) //Back application
        {
            if (myCombinator == null)
            { //You are a parenthesis
                print("applying parens to goal");
                int my_index;
                for (my_index = 0; my_index < target.childCount; my_index++)
                {
                    if (transform.position.x < target.GetChild(my_index).position.x)
                    {
                        break;
                    }
                }


                if (my_index != target.childCount)
                    return Util.BackApplyParen(spawnTarget.goal, index.Skip(1).ToList(), my_index).Match(t =>
                    {
                        PreviewRedundantParenInfo prp = new PreviewRedundantParenInfo(index,my_index,spawnTarget, () =>
                        {

                            spawnTarget.addParens(index.Skip(1).ToList(), my_index, GetComponent<LayoutTracker>(), t);

                            PlaceMe();
                            myDraggableType = DraggableHolder.DraggableType.RedundantParens;
                        },UnPlace);
                        return Sum<Action,PreviewInfo>.Inr(PreviewInfo.In2(prp));
                        
                    }, _ =>
                    {
                        print(
                            $"goal: {spawnTarget.GetComponent<SymbolManagerTester>().show(spawnTarget.goal)},myindex: {my_index}, path {index.Select(c => c.ToString()).Aggregate((a, b) => $"{a}, {b}")}");
                        return Sum<Action, PreviewInfo>.Inl(DestroyMe);
                    });
                else
                    return Sum<Action, PreviewInfo>.Inl(DestroyMe);
            }
            else
            { //You not are a parenthesis
                return Util.BackApply(spawnTarget.goal, myCombinator, index.Skip(1).ToList()).Match(t =>
                        
                    Sum<Action,PreviewInfo>.Inr(PreviewInfo.In1(new PreviewBackCombinatorInfo(spawnTarget,parenTracker,index,() => {

                        pushUndoGoalTerm.Invoke(spawnTarget.goal);


                        spawnTarget.unApply(t, GetComponent<LayoutTracker>(), myCombinator, index.Skip(1).ToList());

                        PlaceMe();
                        myDraggableType = DraggableHolder.DraggableType.NoDragging;

                    })))    
                   
                , _ => Sum<Action, PreviewInfo>.Inl(DestroyMe));
            }
            
        }
        else if(!spawnTarget && !NoForwardMode.val && target.GetComponentInParent<DraggableHolder>() && target.GetComponentInParent<DraggableHolder>().myType == DraggableHolder.DraggableType.Proposal) //forward application
        {
            if (evaluationMode)
            {
                return Sum<Action, PreviewInfo>.Inl(DestroyMe);
            }

			int my_index;
            for (my_index = 0; my_index < target.childCount; my_index++)
            {
                if (transform.position.x < target.GetChild(my_index).position.x)
                {
                    break;
                }
            }
            var sm = target.GetComponentInParent<SymbolManager>();
            return Sum<Action, PreviewInfo>.Inr(
                PreviewInfo.In0(new PreviewForwardInfo(sm,index,target,my_index, () =>{
            
                        //if (pushUndoProposalTerm) {
                        //	pushUndoProposalTerm.Invoke(sm.readTerm());
                        //} else {
                        //	Debug.LogError("pushUndoProposalTerm is null in DraggableSpell: " + this);
                        //}
                        sm.Insert(index.Skip(1).Append(my_index).ToList(), my_term);

                        // paren = AccessTransfrom(topTracker, paren_index);

                        transform.SetParent(target, true);
                        transform.SetSiblingIndex(my_index);
                        PlaceMe();
                        myDraggableType = DraggableHolder.DraggableType.Proposal;
                
                    }, UnPlace))
            );
           
        }
        return Sum<Action, PreviewInfo>.Inl(DestroyMe);
    }
    
    private Tuple<SpawnTarget,List<Transform>> getTargets()
    {
        bool GoodTarget(GameObject g)
        {
            char[] c = {'>'};
            if (myCombinator == null || myCombinator.lambdaTerm.Split(c)[1].SkipWhile(char.IsWhiteSpace).Count() > 1)
                return g.CompareTag("ParenSymbol") && g != gameObject;

            return g.GetComponent<LayoutTracker>() && g.GetComponentInParent<SpawnTarget>();
        }
        
        PointerEventData data = new PointerEventData(es);
        data.position = Input.mousePosition;
        List<RaycastResult> L = new List<RaycastResult>();
        gr.Raycast(data,L);

        var Result = L.Select(l => l.gameObject).Where(GoodTarget).Select(l => l.transform).ToList();

        if (Result.Count == 0)
            return Tuple.Create((SpawnTarget)null, Result);
        
        return Tuple.Create(Result[0].gameObject.GetComponentInParent<SpawnTarget>(),Result);
    }
    
    private Transform AccessTransfrom(Transform t,List<int> path)
    {
        foreach (int i in path)
        {
            t = t.GetChild(i);
        }

        return t;
    }
}
