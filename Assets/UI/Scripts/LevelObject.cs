using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelObject : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private int levelNum;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private TextMeshProUGUI numText;
    [SerializeField] private TextMeshProUGUI descriptionText;

#pragma warning restore 0649

    private LevelSelect levelSelect;

    public void SetLevelSelect(LevelSelect ls)
    {
        levelSelect = ls;
    }
    
    public void SetLevelNum(int num)
    {
        levelNum = num;
    }

    public void SetCurrentLevel()
    {
        levelLoader.setLevelIndex(levelNum );
    }

    public void ButtonHit()
    {
        SetCurrentLevel();
        levelLoader.loadSelectedLevel();
    }

    public void StoryButton()
    {
        SetCurrentLevel();
        levelSelect.OpenStory();
    }

    public void SetData(int num, string description)
    {
        numText.text = "Level " + num;
        descriptionText.text = description;
    }
}
