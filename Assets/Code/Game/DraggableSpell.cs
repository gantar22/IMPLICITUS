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

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        gr = GetComponentInParent<GraphicRaycaster>();
        es = GetComponentInParent<EventSystem>();
        onApply.AddListener(() => { enabled = false; });
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

        while (L.Count > 0 && (!L[0].gameObject.CompareTag("ParenSymbol") || L[0].gameObject == gameObject))
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
             /* TODO check to see if the paren is in the build space and not the goal space */
            {
                /*      Insert yourself into the term       */
                
                Transform paren = L[0].gameObject.transform;
                var parenTracker = paren.GetComponent<LayoutTracker>();

                
                int my_index;
                for (my_index = 0; my_index < paren.childCount; my_index++)
                {
                    if (transform.position.x < paren.GetChild(my_index).position.x)
                    {
                        break;
                    }
                }

                List<int> paren_index = parenTracker.index;
                var my_term = myCombinator == null
                    ? Shrub<Sum<Combinator, Variable>>.Node(new List<Shrub<Sum<Combinator, Variable>>>())
                    : Shrub<Sum<Combinator, Variable>>.Leaf(Sum<Combinator, Variable>.Inl(myCombinator));
                var sm = paren.GetComponentInParent<SymbolManager>();
                sm.Insert(paren_index.Skip(1).Append(my_index).ToList(),my_term);

                
               // paren = AccessTransfrom(topTracker, paren_index);
                
                transform.SetParent(paren,true);
                transform.SetSiblingIndex(my_index);
                GetComponent<LayoutTracker>().root = parenTracker.root;
                GetComponent<LayoutTracker>().enabled = true;
                this.enabled = false;

            }
        }

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
