using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(Image))]
public class HighlightParen : MonoBehaviour, IHighlightable
{
    private float power;
    private Image im;
    private Color oldColor;
    private readonly Color highlightColor = new Color(1f, 0.63f, 0.15f);
    private Color newColor;
    private IEnumerator myloop;

    private void Awake()
    {
        newColor = highlightColor;
        myloop = loop();
    }

    // Start is called before the first frame update
    void Start()
    {
        im = GetComponent<Image>();
        oldColor = Color.white; // yuck hard coding
        
    }

    private void OnEnable()
    {
        StartCoroutine(myloop);
    }

    private void OnDisable()
    {
        StopCoroutine(myloop);
    }

    IEnumerator loop()
    {
        yield return null;
        while (true)
        {
            if (!im)
                Start();
            if(0 < power)
                im.color = Color.Lerp(im.color,newColor,Time.deltaTime * 10);
            if(power <= 0)
                im.color = Color.Lerp(im.color,oldColor,Time.deltaTime * 30);
            
            
            yield return null;
            if (!transform.parent.GetComponent<LayoutTracker>() && GetComponentsInChildren<LayoutTracker>().Any(lt => lt.gameObject != gameObject))
            {
                oldColor = Color.Lerp(oldColor,  new Color(1,1,1,0),  Time.deltaTime * 40);
            }
            else
            {
                oldColor = Color.Lerp(oldColor,Color.white, Time.deltaTime * 40);
            }
        }
    }

    public void select()
    {
        power = 1;
    }

    public void unselect()
    {
        power = 0;
    }



}
