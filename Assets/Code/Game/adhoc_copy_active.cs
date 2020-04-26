using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class adhoc_copy_active : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    CanvasGroup myCG;
    // Start is called before the first frame update
    void Start()
    {
        myCG = gameObject.AddComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        myCG.alpha = canvasGroup.alpha;
        myCG.interactable = canvasGroup.interactable;
        myCG.blocksRaycasts = canvasGroup.blocksRaycasts;
    }
}
