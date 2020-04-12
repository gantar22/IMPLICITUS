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
    public IEnumerator createLevelCompleteScreen()
    {
        yield return new WaitForSeconds(1);
        GameObject completeScreen = (GameObject)Instantiate(completedScreenPrefab,
                                                            parentCanvasTransform);
    }

    void Start()
    {
        //Initiate the Listener to create screen
        complete.AddRemovableListener(_ => StartCoroutine(createLevelCompleteScreen()),this);
    }

}
