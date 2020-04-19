using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lambda;
using TypeUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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


    private struct PreviewRedundantParenInfo
    {
        private List<int> path;
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
            List<int> index = GetComponent<LayoutTracker>().index;
            SymbolManager sm = GetComponentInParent<SymbolManager>();
            var lt = sm.RemoveAtAndReturn(index.Skip(1).ToList(),sm.GetComponentInChildren<LayoutTracker>());
            lt.transform.SetParent(GetComponentInParent<Canvas>().transform,true);
            lt.transform.SetSiblingIndex(GetComponentInParent<Canvas>().transform.childCount - 1);
            lt.enabled = false;
        }
        
        offset = transform.position;
		hasBeenDragged = true;
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
                MakeDrop(spawnTarget, L[0]).Match(f => f(), s => s());
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
                    succ();
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

    public Sum<Action,Action> MakeDrop(SpawnTarget spawnTarget, Transform target) //left is on failure, right is on success
    {
        void PlaceMe()
        {
            if(myDraggableType == DraggableHolder.DraggableType.LeftPane)    
                duplicate.enabled = true;
            
            var lt = GetComponent<LayoutTracker>();
            lt.enabled = true;
            lt.root = lt.GetComponentInParent<SymbolManager>().skeletonRoot;
        }

        var parenTracker = target.GetComponent<LayoutTracker>();
        
        List<int> index = parenTracker.index;
    
        var my_term = myCombinator == null
            ? Shrub<Sum<Combinator, Variable>>.Node(new List<Shrub<Sum<Combinator, Variable>>>())
            : Shrub<Sum<Combinator, Variable>>.Leaf(Sum<Combinator, Variable>.Inl(myCombinator));
        if (!target.GetComponentInParent<DraggableHolder>())
        {
            return Sum<Action,Action>.Inl(DestroyMe);
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
                        return Sum<Action, Action>.Inr(() =>
                        {

                            spawnTarget.addParens(index.Skip(1).ToList(), my_index, GetComponent<LayoutTracker>(), t);

                            PlaceMe();
                            myDraggableType = DraggableHolder.DraggableType.RedundantParens;
                        });
                    }, _ =>
                    {
                        print(
                            $"goal: {spawnTarget.GetComponent<SymbolManagerTester>().show(spawnTarget.goal)},myindex: {my_index}, path {index.Select(c => c.ToString()).Aggregate((a, b) => $"{a}, {b}")}");
                        return Sum<Action, Action>.Inl(DestroyMe);
                    });
                else
                    return Sum<Action, Action>.Inl(DestroyMe);
            }
            else
            { //You not are a parenthesis
                return Util.BackApply(spawnTarget.goal, myCombinator, index.Skip(1).ToList()).Match(t =>
                    Sum<Action, Action>.Inr(() =>
                    {

                        pushUndoGoalTerm.Invoke(spawnTarget.goal);


                        spawnTarget.unApply(t, GetComponent<LayoutTracker>(), myCombinator, index.Skip(1).ToList());

                        PlaceMe();
                        myDraggableType = DraggableHolder.DraggableType.NoDragging;

                    })
                , _ => Sum<Action, Action>.Inl(DestroyMe));
            }
            
        }
        else if(!spawnTarget && !NoForwardMode.val && target.GetComponentInParent<DraggableHolder>() && target.GetComponentInParent<DraggableHolder>().myType == DraggableHolder.DraggableType.Proposal) //forward application
		{
			if (evaluationMode)
            {
                return Sum<Action, Action>.Inl(DestroyMe);
            }

			int my_index;
            for (my_index = 0; my_index < target.childCount; my_index++)
            {
                if (transform.position.x < target.GetChild(my_index).position.x)
                {
                    break;
                }
            }
        
            return Sum<Action, Action>.Inr(() =>
            {
            
                var sm = target.GetComponentInParent<SymbolManager>();
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
                
            });
           
        }
        return Sum<Action, Action>.Inl(DestroyMe);
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
