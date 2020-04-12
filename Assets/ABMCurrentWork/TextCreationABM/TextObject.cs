using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "newTextObject", menuName = "Text Object")]
public class TextObject : ScriptableObject 
{
    public TextAsset writtenText; //Will contain the text file inputed into the object
 
    public int scrollSpeed; //Scroll speed of text

    
    public string getText()
    {
        return writtenText.text;
    }

}
