using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Implicitus/New Chapter List")]
public class ChapterList : ScriptableObject
{
    [SerializeField] private Chapter[] _chapters;

    public Chapter[] Chapters
    {
        get
        {
            return _chapters;
        }
    }
}
