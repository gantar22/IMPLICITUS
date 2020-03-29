using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lambda;
using TypeUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableSpell : MonoBehaviour, IDragHandler, IBeginDragHandler, IDropHandler
{
    private Vector2 offset = Vector2.right;
    private RectTransform rt;
    private GraphicRaycaster gr;
    private EventSystem es;
    private Image duplicate;
    public Combinator myCombinator;
    public UnitEvent onApply;
    public UnitEvent onUnapply;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        gr = GetComponentInParent<GraphicRaycaster>();
        es = GetComponentInParent<EventSystem>();
        onApply.AddRemovableListener(_ => { enabled = false; },this);
        onUnapply.AddRemovableListener(_ => { enabled = true; }, this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rt.position = Camera.main.ScreenToWorldPoint(Vector3.forward * 10 + Input.mousePosition);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //create duplicate and do some sibling index stuff and layout element stuff
        duplicate = Instantiate(gameObject,transform.parent).GetComponent<Image>();
        duplicate.transform.SetSiblingIndex(transform.GetSiblingIndex());
        duplicate.enabled = false;
        GetComponent<LayoutElement>().ignoreLayout = true;
        offset = transform.position;
    }

    public void OnDrop(PointerEventData eventData)
    {
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
            Destroy(gameObject);
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
                
                if(spawnTarget)
                {
                    if (myCombinator == null)
                    {
                        int my_index;
                        for (my_index = 0; my_index < target.childCount; my_index++)
                        {
                            if (transform.position.x < target.GetChild(my_index).position.x)
                            {
                                break;
                            }
                        }

                        if (Util.BackApplyParen(spawnTarget.goal, index.Skip(1).ToList(), my_index).Match(t =>
                        {
                            spawnTarget.createTarget(t);
                            return true;
                        }, _ => false))
                            break;
                    }
                    else
                    {
                        if (Util.BackApply(spawnTarget.goal, myCombinator, index.Skip(1).ToList()).Match(t =>
                        {
                            spawnTarget.createTarget(t);
                            return true;
                        }, _ => false)) 
                            break;
                        
                    }
                    
                }
                else
                {
                    int my_index;
                    for (my_index = 0; my_index < target.childCount; my_index++)
                    {
                        if (transform.position.x < target.GetChild(my_index).position.x)
                        {
                            break;
                        }
                    }
                
                    var sm = target.GetComponentInParent<SymbolManager>();
                    sm.Insert(index.Skip(1).Append(my_index).ToList(), my_term);

                    // paren = AccessTransfrom(topTracker, paren_index);

                    transform.SetParent(target, true);
                    transform.SetSiblingIndex(my_index);
                    GetComponent<LayoutTracker>().root = parenTracker.root;
                    GetComponent<LayoutTracker>().enabled = true;
                    this.enabled = false;
                    
                    duplicate.enabled = true;
                    return;
                }
            } 
        }
        Destroy(gameObject);
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
