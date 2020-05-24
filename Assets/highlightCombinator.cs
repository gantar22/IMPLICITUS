using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class highlightCombinator : MonoBehaviour, IHighlightable
{
    private Image im;
    [SerializeField] private Color highlightColor;
    private bool selected;
    
    // Start is called before the first frame update
    void Start()
    {
        im = GetComponent<Image>();
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
        var c = selected ? highlightColor : Color.white;
        im.color = Color.Lerp(im.color, c, Time.deltaTime * 10);

    }
}
