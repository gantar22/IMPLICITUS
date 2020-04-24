using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LayoutTracker : MonoBehaviour, IPointerClickHandler
{
    public RectTransform root;

    public List<int> index;
    private RectTransform rt;
    private LayoutElement layoutElement;
    [SerializeField] private bool matchWidth = false;
    private readonly bool debug = false;
    private RectTransform dest = null;
    private IEnumerator loop;
    private Vector3 anchor;


    private float height;
    
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        loop = Loop();
    }


    private void Start()
    {
        Transform t = transform;
        index = new List<int>();
        while (t.parent != null && t.parent.GetComponent<LayoutTracker>() != null)
        {
            index.Add(t.GetSiblingIndex());
            t = t.parent;
        }
        index.Add(0);
        index.Reverse();
        transform.position = target(index).position;
        height = rt.sizeDelta.y;
    }

    private void OnEnable()
    {
        recalcIndex();
        anchor = rt.position;
        StartCoroutine(loop);
    }

    private void OnDisable()
    {
        StopCoroutine(loop);
    }


    public RectTransform target(List<int> i)
    {
        RectTransform t = root;
        foreach (int child in i)
        {
            if(child < t.childCount)
                t = t.GetChild(child).GetComponent<RectTransform>();
        }

        return t;
        //oof performance
        //use transfom.hasChanged thingy TODO
    }



    void TravelTo(Spell spell,int myArgIndex)
    {
        
    }

    void recalcIndex()
    {
        Transform t = transform;
        index = new List<int>();
        while (t.parent != null && t.parent.GetComponent<LayoutTracker>() != null)
        {
            index.Add(t.GetSiblingIndex());
            t = t.parent;
        }
        index.Add(0);
        index.Reverse();
    }
    
    IEnumerator Loop()
    {
        anchor = rt.position;
        while (true)
        {
            anchor = rt.position;
            yield return null;
            rt.position = anchor;
            yield return new WaitForEndOfFrame();
            rt.position = anchor;
            //Carter's system just doesn't work and he's subverting it entirely

            while (dest == null || dest.hasChanged)
            {
                recalcIndex();
                dest = target(index);
                dest.hasChanged = false;
                yield return null;
                rt.position = anchor;
                yield return null;
                rt.position = anchor;
            }

            var delt = (dest.position - transform.position);
            bool parentMoving = transform.parent.GetComponent<LayoutTracker>() &&
                                transform.parent.GetComponent<LayoutTracker>().Moving();
            
            if(!parentMoving && delt.magnitude > .05f)
                rt.position += Vector3.ClampMagnitude((Time.deltaTime * 5f * delt.magnitude + Time.deltaTime * 3) * delt.normalized,Time.deltaTime * 15);
            if (.01f < Mathf.Abs(delt.y))
                rt.position += Vector3.ClampMagnitude((Time.deltaTime * 5f * delt.magnitude + Time.deltaTime * 25) * delt.normalized,Time.deltaTime * 20);
            
            if (matchWidth)
            {
                var newSize = new Vector2(dest.rect.width,height + Mathf.Max(60 - index.Count * 10,60 / Mathf.Pow(2,index.Count)));

                rt.sizeDelta = Vector2.Lerp(rt.sizeDelta,newSize,Time.deltaTime * 20);
            }
        }
    }

    public bool Moving()
    {
        if (dest == null)
        {
            recalcIndex();
            dest = target(index);
        } 
        return .05f < (transform.position - dest.position).magnitude;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LayoutTracker root = this;
        for (int i = 0; i < index.Count - 1; i++)
            root = root.transform.parent.GetComponent<LayoutTracker>();
        GetComponentInParent<SymbolManager>().HandleClick(index.Skip(1).ToList(),root);
    }
}
