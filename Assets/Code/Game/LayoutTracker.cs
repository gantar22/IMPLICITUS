﻿using System;
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


    private float height;
    
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
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

    // Update is called once per frame
    void Update()
    {

        //Carter's system just doesn't work and he's subverting it entirely
        if (debug)
        {
            Transform t = transform;
            bool readjust = false;
            List<int> indices = new List<int>();
            foreach (var sib in index.Skip(1).Reverse())
            {
                if (t.GetSiblingIndex() != sib || t.GetComponent<LayoutTracker>() == null)
                {
                    // Debug.LogError($"BAD INDEX AT {gameObject}");
                    readjust = true;
                }
                indices.Add(t.GetSiblingIndex());
                t = t.parent;
            }

            if (readjust || t.GetComponent<LayoutTracker>() == null)
                index = indices.Skip(0).Reverse().ToList().Prepend(0).ToList();
        }

        if (transform.hasChanged || dest == null)
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
            dest = target(index);
        }

        var delt = (dest.position - transform.position);
        if(delt.magnitude > .05f)
            rt.position += 2f * Time.deltaTime * delt + Time.deltaTime * delt.normalized;
        if (matchWidth)
        {
            rt.sizeDelta = new Vector2(dest.rect.width,height + Mathf.Max(60 - index.Count * 10,60 / Mathf.Pow(2,index.Count)));
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
