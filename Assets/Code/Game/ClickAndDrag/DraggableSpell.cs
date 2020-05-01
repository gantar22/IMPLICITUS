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
	public CombinatorEvent updateCodexEvent;
	private DraggableHolder.DraggableType myDraggableType;
    [SerializeField]
    private BoolRef evaluationMode;
	[HideInInspector]
    public bool hasBeenDragged = false;
    public BoolRef NoForwardMode;
    private bool oneTry = true;
    private bool shouldMove = false;
    private Vector3 vel = Vector3.zero;

	private CodexOnSpell codexOnSpell;
	private Sum<PreviewInfo,Unit> previewState = Sum<PreviewInfo,Unit>.Inr(new Unit());

    private Vector2 mouseVel;
    
    [SerializeField] IntEvent effectAudioEvent; //Event Calls audio sound

    public struct PreviewRedundantParenInfo
    {// keep as long as your index is the same
        public List<int> path;
        public int my_index;
        public SpawnTarget spawnTarget;
        public float timeFound;
        public Vector2 mousePositionWhenFound;
        public Action place;
        public Action unplace;

        public PreviewRedundantParenInfo(List<int> path, int myIndex, SpawnTarget spawnTarget, Action place, Action unplace, float timeFound, Vector2 mousePositionWhenFound)
        {
            this.mousePositionWhenFound = mousePositionWhenFound;
            this.timeFound = timeFound;
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
        public Action highlight;
        public Action unHighlight;

        public PreviewBackCombinatorInfo(SpawnTarget spawnTarget, LayoutTracker paren, List<int> path, Action place, Action highlight, Action unHighlight)
        {
            this.highlight = highlight;
            this.unHighlight = unHighlight;
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

        StartCoroutine(calcVel());
        
        if (false && myDraggableType != DraggableHolder.DraggableType.RedundantParens)
        {
            onApply.AddRemovableListener(_ => { enabled = false; }, this);
            onUnapply.AddRemovableListener(_ => { enabled = true; }, this);
        }

        
    }

    IEnumerator calcVel()
    {
        while (true)
        {
            var pos = Input.mousePosition;
            yield return null;
            mouseVel = Vector2.Lerp(mouseVel, (Input.mousePosition - pos) / Time.deltaTime, Time.deltaTime * 100);
        }
    }

    private bool NoDragging()
    {
        switch (myDraggableType)
        {
            case DraggableHolder.DraggableType.NoDragging:
            case DraggableHolder.DraggableType.RedundantParens when myCombinator != null:
            case DraggableHolder.DraggableType.RedundantParens when transform.GetSiblingIndex() != 0:
                return true;
            case DraggableHolder.DraggableType.Proposal when evaluationMode.val:
                return true;
            default:
                return false;
        }
    }

    private void Update()
    {
        if(shouldMove)
            rt.position = Vector3.SmoothDamp(rt.position,Camera.main.ScreenToWorldPoint(Vector3.forward * 10 + Input.mousePosition),ref vel, .05f);
        
        
    }

    public void OnDrag(PointerEventData eventData) {
        shouldMove = false;
		if (!hasBeenDragged)
            return;


        void move()
        {
            shouldMove = true;
        }

        previewState.Match(
            //should I stop?
            sum => sum.Match(forward => {move();/*within one*/  }, backward =>
            {
                move();/*paren changed*/

                void Fail()
                {
                    backward.unHighlight();
                }
                (var spawnTarget, var targets) = getTargets();
                FreshPreview(spawnTarget, targets).Match(preview => preview.Match(forward => Fail(), newBackward =>
                {
                    if (newBackward.paren != backward.paren)
                        Fail();
                    else
                        backward.highlight();
                    //we'll wait a frame to re-highlight this way

                },redundant => Fail()), _ => Fail());
            }, redundant =>
            {
                void Fail()
                {
                    redundant.unplace();
                    previewState = Sum<PreviewInfo, Unit>.Inr(new Unit());
                }
//call unplace then reverse it TODO
                if (Time.time - redundant.timeFound < .5f && Vector2.Distance(Input.mousePosition, redundant.mousePositionWhenFound) < 150)
                    return;
                if (Vector2.Distance(Input.mousePosition, redundant.mousePositionWhenFound) < 20)
                    return;
                
                (var spawnTarget, var targets) = getTargets(includeSelf:true);
                
                /* path has changed or index != 0 */
                FreshPreview(spawnTarget,targets).Match(preview => preview.Match(_ =>
                {
                    print("forward");
                    Fail();
                }, _ =>
                {
                    print("backward");
                    Fail();
                }, state =>
                {
                    if (state.path.Count == redundant.path.Count && state.path.Select((x, i) => x == redundant.path[i]).All(x => x))
                    {//the path is the same
                        if (state.my_index == 0)
                        {
                            print("same path");
                            /* do nothing */
                        }
                        else
                        {
                            print("path same length, but differs");
                            Fail();
                        }
                    } else if (state.path.Count == redundant.path.Count + 1 &&
                        redundant.path.Select((x, i) => state.path[i] == x).All(x => x))// the new path is one lower
                    {
                        if (state.path.Last() == 0 && state.my_index == targets[0].childCount)//its directly to the right of the last child
                        {
                            print("far right");
                            /* do nothing */
                        }
                        else
                        {
                            print("path is longer, but it's not directly to the left of the right paren");
                            Fail();
                        }
                    } else
                    {
                        print("path is just different");
                        Fail();
                    }
                }), _ =>
                {
                    print("wouldn't count as a placement");
                    if (GetComponent<RectTransform>().rect.max.x + 50 < Input.mousePosition.x)
                        Fail();
                    else if(possibleTargets().Any() && 15 < Mathf.Abs(Input.mousePosition.x - redundant.mousePositionWhenFound.x))
                        Fail();
                    else if (!possibleTargets().Any() &&
                             25 < Vector2.Distance(Input.mousePosition, redundant.mousePositionWhenFound))
                        Fail();
                    else
                    {
                        print("hasn't moved far enough");
                        /* do nothing */
                    }
                });
            })
            , __ =>
        {
            //Should I start?
            var (spawnTarget, targets) = getTargets();
            previewState = FreshPreview(spawnTarget,targets);
            previewState.Match(sum => sum.Match(forward =>{/* place*/} , backward =>
                {
                    /* highlight*/
                    backward.highlight();
                }, redundant =>
                {
                    if(mouseVel.magnitude < 5f)
                        redundant.place();
                    else
                    {
                        previewState = Sum<PreviewInfo, Unit>.Inr(new Unit());
                        move();
                    }
                    
                }),
                _ =>
                { //no preview from last frame, no preview for next frame
                    move();
                });
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
		if (effectAudioEvent) {
			effectAudioEvent.Invoke(20); //Pick Up Sound Effect
		}

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

		// Update the codex
		if (updateCodexEvent) {
			updateCodexEvent.Invoke(myCombinator);
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
        lt.root = lt.GetComponentInParent<SymbolManager>().skeletonRoot;
        lt.enabled = true;
        LayoutRebuilder.MarkLayoutForRebuild(lt.GetComponentInParent<Canvas>().GetComponent<RectTransform>());
        
        SpawnTarget sp = GetComponentInParent<SpawnTarget>();
        sp?.CheckSuccess();
    }
    void UnPlace()
    {
        var pos = transform.position;
        List<int> index = GetComponent<LayoutTracker>().index;
        SymbolManager sm = GetComponentInParent<SymbolManager>();
        SpawnTarget sp = GetComponentInParent<SpawnTarget>();
        var lt = sm.RemoveAtAndReturn(index.Skip(1).ToList(), sm.GetComponentInChildren<LayoutTracker>());
        lt.transform.SetParent(GetComponentInParent<Canvas>().transform,true);
        lt.transform.SetAsLastSibling();
        lt.enabled = false;
        transform.position = pos;
        LayoutRebuilder.MarkLayoutForRebuild(sm.GetComponent<RectTransform>());
        sp?.CheckSuccess();

    }
    
	// WARNING - this seems to not be getting called according to IEndDragHandler because
	//   CodexOnSpell implements IPointerClickHandler, and when they're both on the same GameObject,
	//   it will only call one of them. So instead, OnPointerClick() in CodexOnSpell will call this.
	//   (If there's an issue with this tho, let Dom know)
    public void OnEndDrag(PointerEventData eventData)
    {        
        shouldMove = false;
        Drop(true);
    }
    
    
    public void Drop(bool endDrag = false)
    {
        if (!hasBeenDragged)
                return;
        hasBeenDragged = false;

        if (previewState.Match(sum => sum.Match(_ => false, _ => false, redundant => true), _ => false))
        {
            previewState = Sum<PreviewInfo, Unit>.Inr(new Unit());
            return;
        }
        (var spawnTarget,var L) = getTargets();
        
        if (L.Count == 0)
        {
            if (endDrag)
            {
				if (effectAudioEvent) {
					effectAudioEvent.Invoke(22); //Fizzle Sound Effect
				}
            }
            print("no hits");
            DestroyMe();
        }
        else
        {
            if (endDrag)
            {
                effectAudioEvent.Invoke(21); //Drop Sound Effect
            }
            /*      Insert yourself into the term       */
            if (oneTry)
            {
                MakeDrop(spawnTarget, L[0]).Match(f => f(), s => s.Match(x => x.place(),x => x.place(),x =>
                {
                    previewState =
                        Sum<PreviewInfo, Unit>.Inr(new Unit());
                    DestroyMe();
                }));
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
                        },UnPlace,Time.time,Input.mousePosition);
                        return Sum<Action,PreviewInfo>.Inr(PreviewInfo.In2(prp));
                        
                    }, _ =>
                    {
                        return Sum<Action, PreviewInfo>.Inl(DestroyMe);
                    });
                else
                    return Sum<Action, PreviewInfo>.Inl(DestroyMe);
            }
            else
            { //You not are a parenthesis
                HighlightParen hightligher = parenTracker.GetComponent<HighlightParen>();
                return Util.BackApply(spawnTarget.goal, myCombinator, index.Skip(1).ToList()).Match(t =>
                        
                    Sum<Action,PreviewInfo>.Inr(PreviewInfo.In1(new PreviewBackCombinatorInfo(spawnTarget,parenTracker,index,() => {

                        pushUndoGoalTerm.Invoke(spawnTarget.goal);


                        spawnTarget.unApply(t, GetComponent<LayoutTracker>(), myCombinator, index.Skip(1).ToList());

                        PlaceMe();
                        hightligher.toggleOff();
                        myDraggableType = DraggableHolder.DraggableType.NoDragging;

                    },hightligher.toggleOn, hightligher.toggleOff)))    
                   
                , _ => Sum<Action, PreviewInfo>.Inl(DestroyMe));
            }
            
        }
        else if(!spawnTarget && !NoForwardMode.val && target.GetComponentInParent<DraggableHolder>() && target.GetComponentInParent<DraggableHolder>().myType == DraggableHolder.DraggableType.Proposal) //forward application
        {
            if (evaluationMode.val)
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

    private List<GameObject> possibleTargets()
    {
        PointerEventData data = new PointerEventData(es);
        data.position = Input.mousePosition;
        List<RaycastResult> L = new List<RaycastResult>();
        gr.Raycast(data,L);
        return L.Select(r => r.gameObject).ToList();
    }
    
    private Tuple<SpawnTarget,List<Transform>> getTargets(bool includeSelf=false)
    {
        bool GoodTarget(GameObject g)
        {
            char[] c = {'>'};
            if (myCombinator == null || myCombinator.lambdaTerm.Split(c)[1].SkipWhile(char.IsWhiteSpace).Count() >= 1)
                return g.CompareTag("ParenSymbol") && (g != gameObject || includeSelf);

            return g.GetComponent<LayoutTracker>() && g.GetComponentInParent<SpawnTarget>();
        }
        

        var Result = possibleTargets().Where(GoodTarget).Select(l => l.transform).ToList();

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
