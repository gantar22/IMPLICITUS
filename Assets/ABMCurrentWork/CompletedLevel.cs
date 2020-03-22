using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompletedLevel : MonoBehaviour
{
    /* Variables */

    [SerializeField] UnitEvent complete; //Event that will listen for level completed

    [SerializeField] GameObject completedScreenPrefab; //Hold prefab of level complete screen
    [SerializeField] Transform parentCanvasTransform; //Parent Canvas that will be used


    /* Functions */
    // Creates the Level Complete Screen
    public void createLevelCompleteScreen()
    {
        GameObject completeScreen = (GameObject)Instantiate(completedScreenPrefab,
                                                            parentCanvasTransform);
    }

    void Start()
    {
        //Initiate the Listener to create screen
        complete.AddListener(createLevelCompleteScreen);
    }

}
