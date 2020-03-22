using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreateText : MonoBehaviour
{
    public TextObject textObject;  //Object holding information for text
    private TMP_Text textMesh;  //Gets text mesh pro object; To access text
    private RectTransform rect;    //Holds Transform of text object


    [SerializeField] UnitEvent createText;     //feidl
    [SerializeField] UnitEvent goesOverBounds; //Tells when we have gone over bounds

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        rect = GetComponent<RectTransform>();
        
        //Apply text in IntroText, to Text in TextMeshPro component ensuring no null reference
        if (textObject != null){
            textMesh.text = textObject.getText();  //Get content from text
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Transforms the text upward in the UI
        rect.transform.position += transform.up * textObject.scrollSpeed * Time.deltaTime;
        
        //Goes over a certain position
        if (rect.transform.position.y > 125)
        {
            goesOverBounds.Invoke();
        }
    }
}
