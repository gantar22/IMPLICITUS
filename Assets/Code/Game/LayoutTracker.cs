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
    private IEnumerator antiloop;
    private Vector3 anchor;
    private Vector3 velocity;

    private Transform oldRoot;

    private float height;
    
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        loop = Loop();
        antiloop = resize();
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
        if(gameObject.GetComponent<Image>()?.enabled != null && gameObject.activeInHierarchy)
            gameObject.GetComponent<Image>().StopCoroutine(antiloop);
        StartCoroutine(loop);
    }

    private void OnDisable()
    {
        if(gameObject.activeInHierarchy)
            StopCoroutine(loop);
        if(gameObject.GetComponent<Image>()?.enabled != null && gameObject.activeInHierarchy)
            gameObject.GetComponent<Image>().StartCoroutine(antiloop);
    }


    IEnumerator resize()
    {
        while (true)
        {
            yield return null;
            if (matchWidth)
            {
                rt.sizeDelta = Vector2.Lerp(rt.sizeDelta,new Vector2(100,100),Time.deltaTime * 20);
            }
        }
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


    public List<Transform> parents(Transform t)
    {
        if (t.parent != null)
            return parents(t.parent).Prepend(t).ToList();
        else 
            return new List<Transform>().Prepend(t).ToList();
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
        while(true)
        {
            anchor = rt.position;
            yield return null;          
            
            bool scrolling = parents(transform).First(t => !t.GetComponent<LayoutTracker>()).hasChanged;
            bool parentMoving = transform.parent.GetComponent<LayoutTracker>() &&
                                transform.parent.GetComponent<LayoutTracker>().Moving();

            parents(transform).First(t => !t.GetComponent<LayoutTracker>()).hasChanged = false;
            
            //Carter's system just doesn't work and he's subverting it entirely
            

            if(dest == null || dest.hasChanged || oldRoot != root)
            {
                oldRoot = root;
                recalcIndex();
                dest = target(index);
                dest.hasChanged = false;
                while (dest == null || dest.hasChanged )
                {
                    recalcIndex();
                    dest = target(index);
                    dest.hasChanged = false;
                    print("$");
                    yield return null;
                    rt.position = anchor;
                    yield return null;
                    rt.position = anchor;
                }
            }
            //rt.position = anchor;

            var delt = (dest.position - transform.position);

            
            if(!scrolling && parentMoving)
                rt.position = anchor;
            if(delt.magnitude > .001f)
                rt.position = Vector3.SmoothDamp(rt.position, dest.position, ref velocity, .2f, 35);


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
            return true;
        } 
        return .25f < (transform.position - dest.position).magnitude;
    }


    public void LockDown(float t)
    {
        StartCoroutine(lockDown(t));
    }

    IEnumerator lockDown(float t)
    {
        for(float dur = t; 0 < dur; dur -= Time.deltaTime)
        {
            var pos = transform.position;
            yield return null;
            transform.position = pos;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        LayoutTracker root = this;
        for (int i = 0; i < index.Count - 1; i++)
            root = root.transform.parent.GetComponent<LayoutTracker>();
        GetComponentInParent<SymbolManager>().HandleClick(index.Skip(1).ToList(),root);
    }
}
