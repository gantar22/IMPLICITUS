using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimDuringEval : MonoBehaviour
{
    [SerializeField] private BoolRef evalmode;

    [SerializeField] private CanvasGroup canvasGroup;
    

    // Update is called once per frame
    void Update()
    {
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, evalmode.val ? .33f : 1, Time.deltaTime * 2);
        canvasGroup.interactable = !evalmode.val;
    }
}
