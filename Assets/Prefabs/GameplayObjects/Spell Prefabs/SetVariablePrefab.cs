using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVariablePrefab : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private BoolRef forwardmode;
    private int index;

    public void Set(int i)
    {
        index = i;
        GetComponent<UnityEngine.UI.Image>().sprite = sprites[i + 1];
    }


    public void Update()
    {
        GetComponent<UnityEngine.UI.Image>().color = !forwardmode.val ? new Color(0.16f, 0.5f, 0.52f) : new Color(0.97f, 0.95f, 0.54f);
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
