using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LayoutTracker : MonoBehaviour
{
    public RectTransform root;

    public List<int> index;
    private RectTransform rt;
    private LayoutElement layoutElement;
    [SerializeField] private bool matchWidth = false;
    private readonly bool debug = true;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public RectTransform target(List<int> i)
    {
        RectTransform t = root;
        foreach (int child in i)
        {
            t = t.GetChild(child).GetComponent<RectTransform>();
        }

        return t;
        //oof performance
        //use transfom.hasChanged thingy TODO
    }


    // Update is called once per frame
    void Update()
    {
        rt.position += (target(index).position - transform.position) * Time.deltaTime * 5;
        if (matchWidth)
        {
            rt.sizeDelta = new Vector2(target(index).rect.width,rt.rect.height);
        }
        //Carter's system just doesn't work and he's subverting it entirely
        if (debug)
        {
            Transform t = transform;
            bool readjust = false;
            List<int> indices = new List<int>();
            foreach (var sib in index.Skip(1).Reverse())
            {
                if (t.GetSiblingIndex() != sib)
                {
                    // Debug.LogError($"BAD INDEX AT {gameObject}");
                    readjust = true;
                }
                indices.Add(t.GetSiblingIndex());
                t = t.parent;
            }

            if (readjust)
                index = indices.Skip(0).Reverse().ToList().Prepend(0).ToList();

        }
        
    }
}
