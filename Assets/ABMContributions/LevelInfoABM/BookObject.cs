using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Holds the chapters of the game
[System.Serializable]
public struct Chapter
{
    string title;

    //Name of the chapter
    [SerializeField] private string chapterName;

    //Level Description; Uses for narrative or pratical purposes
    [SerializeField] private string description;

    //List of Chapter's levels
    [SerializeField] private List<LevelObject> levels;
}

[CreateAssetMenu(fileName = "newBookObject", menuName = "BookObject")]
public class BookObject : ScriptableObject
{
    /* Data Field */

    //List of the Chapters which hold the levels
    [SerializeField] private List<Chapter> chapters;

    //Holds the current chapter
    public int currentChapter;

    /* Functions */


}
