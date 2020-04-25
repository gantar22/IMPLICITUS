using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class adhoc_toggleImage : MonoBehaviour
{
    public Sprite first;

    public Sprite second;

    public void toggle()
    {
        if (GetComponent<Image>().sprite == first)
            GetComponent<Image>().sprite = second;
        else
            GetComponent<Image>().sprite = second;
    }
    
}
