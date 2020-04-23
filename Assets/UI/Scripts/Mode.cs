using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mode : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private CanvasGroup inputPane;
    [SerializeField] private CanvasGroup targetPane;
    [SerializeField] private bool toggled = false;
#pragma warning restore 0649

    private void Awake()
    {
        toggled = !toggled;
        Toggle();
    }

    public void Toggle()
    {
        if (toggled)
        {
            //TODO animate
            targetPane.alpha = 0;
            targetPane.blocksRaycasts = false;
            targetPane.interactable = false;
            targetPane.GetComponent<LayoutElement>().ignoreLayout = true;
            inputPane.alpha = 1;
            inputPane.blocksRaycasts = true;
            inputPane.interactable = true;
            inputPane.GetComponent<LayoutElement>().ignoreLayout = false;
            toggled = false;
        }
        else
        {
            //TODO animate
            inputPane.alpha = 0;
            inputPane.blocksRaycasts = false;
            inputPane.interactable = false;
            inputPane.GetComponent<LayoutElement>().ignoreLayout = true;
            targetPane.alpha = 1;
            targetPane.blocksRaycasts = true;
            targetPane.interactable = true;
            targetPane.GetComponent<LayoutElement>().ignoreLayout = false;
            toggled = true;
        }
    }
}
