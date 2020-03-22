using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopText : MonoBehaviour
{
    [SerializeField] UnitEvent goneOverBounds;
    private RectTransform rect;    //Holds Transform of text object

    //Function that offsets texts(hopefully puts it under canvas so it can scroll past again)
    void RecreateText()
    {
        rect.transform.position -= transform.up * 250;
    }

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        goneOverBounds.AddListener(RecreateText);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
