using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mode : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private CanvasGroup inputPane;
    [SerializeField] private CanvasGroup targetPane;
    //[SerializeField] private bool toggled = false;
    [SerializeField] private BoolRef forwardMode;
    [SerializeField] private BoolRef noforward;
#pragma warning restore 0649

    private void Awake()
    {
        forwardMode.val = false;
        Toggle();
    }

    private void Start()
    {
        
        if (noforward.val)
        {
            Toggle();
            print("no forward mode");           
        }
    }
    
    

    public void Toggle()
    {
        if (!forwardMode.val)
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
        }
        forwardMode.val = !forwardMode.val;
    }
}
