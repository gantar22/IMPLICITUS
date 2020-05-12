using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lambda;
using TypeUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IHighlightable
{
    void select();
    void unselect();
}
public class SetVariablePrefab : MonoBehaviour, IHighlightable
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private BoolRef forwardmode;
    private int index;
    private Color forwardColor = new Color(0.16f, 0.5f, 0.52f);
    private Color backwardColor = new Color(0.97f, 0.95f, 0.54f);
    private bool selected = false;
    private Image im;
    public void Set(int i)
    {
        index = i;
        im = GetComponent<Image>();
        im.sprite = sprites[i + 1];
    }


    public void select()
    {
        selected = true;
    }

    public void unselect()
    {
        selected = false;
    }
    
    public void Update()
    {
        var c =  !forwardmode.val ? forwardColor : backwardColor;
        if(selected)
            c = Color.red;;
        im.color = Color.Lerp(im.color, c, Time.time);

    }

    /* Prior Replaced Code \\CAN DELETE\\
    [SerializeField] private Color[] colors;
    private int index;
    public void Set( int i)
    {
        index = i;
        GetComponent<UnityEngine.UI.Image>().color = colors[i];
    }
    */
}
