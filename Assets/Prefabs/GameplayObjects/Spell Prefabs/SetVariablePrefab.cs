using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVariablePrefab : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private int index;

    public void Set(int i)
    {
        index = i;
        GetComponent<UnityEngine.UI.Image>().sprite = sprites[i + 1];
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
