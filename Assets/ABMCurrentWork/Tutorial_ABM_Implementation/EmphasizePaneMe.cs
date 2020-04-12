using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmphasizePaneMe : MonoBehaviour
{
    [SerializeField] IntEvent emphasized;
    [SerializeField] int me; //Currently who "me" determined w/ serializefield

    CanvasGroup group; //Will store canvas group of object (for convinience)

    void Start()
    {
        //Stores canvas group of object in variable
        group = GetComponent<CanvasGroup>();
        
        emphasized.AddListener(emphasizePane);
    }


    //Tells specific pane to emphaisze or unemphaisze
    private void emphasizePane(int paneNum)
    {
        //If Pane being emphasized is this pane
        if (me == paneNum)
        {
            //If already emphsized, unemphasize
            if (group.ignoreParentGroups)
            {
                group.ignoreParentGroups = false;
            }
            //If not emphasized, emphasize
            else
            {
                group.ignoreParentGroups = true;
            }
        
        }
    }

}
