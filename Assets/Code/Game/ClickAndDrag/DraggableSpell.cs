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
    public void OnEndDrag(PointerEventData eventData) {
		if (!hasBeenDragged)
            return;
        hasBeenDragged = false;
        
        PointerEventData data = new PointerEventData(es);
        data.position = Input.mousePosition;
        List<RaycastResult> L = new List<RaycastResult>();
        gr.Raycast(data,L);

        bool GoodTarget(GameObject g)
        {
            char[] c = {'>'};
            if (myCombinator == null || myCombinator.lambdaTerm.Split(c)[1].SkipWhile(char.IsWhiteSpace).Count() > 1)
                return g.CompareTag("ParenSymbol") && g != gameObject;

            return g.GetComponent<LayoutTracker>() && g.GetComponentInParent<SpawnTarget>();
        }
        
        while (L.Count > 0 && !GoodTarget(L[0].gameObject))
        {
            L.RemoveAt(0);
        }
        
        if (L.Count == 0)
        {
            print("no hits");
        }
        else
        {
            /*      Insert yourself into the term       */

            
            var spawnTarget = L[0].gameObject.GetComponentInParent<SpawnTarget>();
            foreach (var target in L.Where(l => GoodTarget(l.gameObject)).Select(l => l.gameObject.transform))
            {
                var parenTracker = target.GetComponent<LayoutTracker>();
                
                List<int> index = parenTracker.index;
            
                var my_term = myCombinator == null
                    ? Shrub<Sum<Combinator, Variable>>.Node(new List<Shrub<Sum<Combinator, Variable>>>())
                    : Shrub<Sum<Combinator, Variable>>.Leaf(Sum<Combinator, Variable>.Inl(myCombinator));
                if (!target.GetComponentInParent<DraggableHolder>())
                    continue;
                
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


                        if (my_index != target.childCount && Util.BackApplyParen(spawnTarget.goal, index.Skip(1).ToList(), my_index).Match(t =>
                        {
                            var lt = GetComponent<LayoutTracker>();
                            lt.enabled = true;
                            
                            spawnTarget.addParens(index.Skip(1).ToList(),my_index,lt,t);
                            lt.root = lt.GetComponentInParent<SymbolManager>().skeletonRoot;
                            if(myDraggableType == DraggableHolder.DraggableType.LeftPane)    
                                duplicate.enabled = true;
                            
                            myDraggableType = DraggableHolder.DraggableType.RedundantParens;
                            return true;
                        }, _ =>
                        {
                            print($"goal: {spawnTarget.GetComponent<SymbolManagerTester>().show(spawnTarget.goal)},myindex: {my_index}, path {index.Select(c => c.ToString()).Aggregate((a,b) => $"{a}, {b}")}");
                            return false;
                        }))
                            return;
                    }
                    else
                    { //You not are a parenthesis
                        if(Util.BackApply(spawnTarget.goal, myCombinator, index.Skip(1).ToList()).Match(t =>
                        {
							pushUndoGoalTerm.Invoke(spawnTarget.goal);
                            LayoutTracker lt = GetComponent<LayoutTracker>();
                            lt.enabled = true;
                            spawnTarget.unApply(t,lt,myCombinator,index.Skip(1).ToList());
                            lt.root = GetComponentInParent<SymbolManager>().skeletonRoot;
                            if(myDraggableType == DraggableHolder.DraggableType.LeftPane)    
                                duplicate.enabled = true;
                            
                            
                            myDraggableType = GetComponentInParent<DraggableHolder>().myType;
                            return true;
                        }, _ => false))
                            return;
                    }
                    
                }
                else if(!spawnTarget && !NoForwardMode.val) //forward application
                {
                    if (evaluationMode)
                    {
                        Debug.Break();
                        continue;
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
					//if (pushUndoProposalTerm) {
					//	pushUndoProposalTerm.Invoke(sm.readTerm());
					//} else {
					//	Debug.LogError("pushUndoProposalTerm is null in DraggableSpell: " + this);
					//}
					sm.Insert(index.Skip(1).Append(my_index).ToList(), my_term);

                    // paren = AccessTransfrom(topTracker, paren_index);

                    transform.SetParent(target, true);
                    transform.SetSiblingIndex(my_index);
                    GetComponent<LayoutTracker>().root = parenTracker.root;
                    GetComponent<LayoutTracker>().enabled = true;
                    //this.enabled = false;
                    
                    if(myDraggableType == DraggableHolder.DraggableType.LeftPane)
                        duplicate.enabled = true;
                    myDraggableType = GetComponentInParent<DraggableHolder>().myType;
                    return;
                }

                if (oneTry)
                    break;
            } 
        }
    
        Destroy(gameObject);
        if(myDraggableType == DraggableHolder.DraggableType.LeftPane)    
            duplicate.enabled = true;
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
