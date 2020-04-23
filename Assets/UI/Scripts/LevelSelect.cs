using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private ChapterList chapters;
    [SerializeField] private IntRef unlockedChapter;
    [SerializeField] private IntRef unlockedLevel;
    [SerializeField] private Transform levelHolder;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private LevelObject levelPrefab;
    [SerializeField] private CanvasGroup levelPopup;
#pragma warning restore 0649

    private Coroutine routine;

    private void Awake()
    {
        if(unlockedChapter.val < 0)
        {
            unlockedChapter.val = 0;
            unlockedLevel.val = 0;
        }
    }

    private void Start()
    {
        LoadLevels();
    }

    private void UpdateProgress()
    {
        if (levelLoader.chapterIndex > unlockedChapter.val)
        {
            unlockedChapter.val = levelLoader.chapterIndex;
            unlockedLevel.val = levelLoader.levelIndex;
        }
        else if(levelLoader.chapterIndex == unlockedChapter.val && levelLoader.levelIndex > unlockedLevel.val)
        {
            unlockedLevel.val = levelLoader.levelIndex;
        }
    }

    public void LoadLevels()
    {
        for(int i = 0; i < levelHolder.transform.childCount; i++)
        {
            if (levelHolder.transform.GetChild(i).name.Contains("Level"))
            {
                Destroy(levelHolder.transform.GetChild(i).gameObject);
            }
        }

        UpdateProgress();

        Chapter currChap = chapters.Chapters[levelLoader.chapterIndex];

        int maxLevels = currChap.Levels.Length;

        if(unlockedChapter.val == currChap.Num)
        {
            maxLevels = unlockedLevel.val;
        }

        for(int i = 0; i < maxLevels; i++)
        {
            LevelObject temp = Instantiate(levelPrefab, levelHolder);
            temp.SetLevelNum(i);
            temp.SetLevelSelect(this);
            temp.SetData(i + 1, currChap.Levels[i].Description);
        }
    }

    public int GetMaxUnlockedChapter()
    {
        return unlockedChapter.val;
    }

    public void OpenStory()
    {
        if(routine == null) routine = StartCoroutine(LoadStoryScene());
    }

    private IEnumerator LoadStoryScene()
    {
        LoadManager.instance.LoadSceneAsync("Dialogue");
        while(DialogueManager.instance == null)
        {
            yield return new WaitForEndOfFrame();
        }
        DialogueManager.instance.PlayDialogue(chapters.Chapters[levelLoader.chapterIndex].Levels[levelLoader.levelIndex].DialogueScript.text);
        routine = null;
    }

    public void OpenPopup()
    {
        //animate
        levelPopup.alpha = 1;
        levelPopup.interactable = true;
        levelPopup.blocksRaycasts = true;
    }

    public void ClosePopup()
    {
        //animate
        levelPopup.alpha = 0;
        levelPopup.interactable = false;
        levelPopup.blocksRaycasts = false;
    }

}
