using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
    [SerializeField] private ChapterDisplay chapterDisplay;
#pragma warning restore 0649

    private Coroutine routine;
    private TextAsset saveFile;

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
        LoadSave();
        LoadLevels();
    }

    private void LoadSave()
    {
        string path = Application.dataPath + "/save.txt";

        if (File.Exists(path))
        {
            StreamReader reader = new StreamReader(path);
            string[] data = reader.ReadToEnd().Split(',');
            unlockedChapter.val = int.Parse(data[0]);
            unlockedLevel.val = int.Parse(data[1]);
            reader.Close();
        }
        else
        {
            unlockedChapter.val = 0;
            unlockedLevel.val = 0;
            levelLoader.chapterIndex = 0;
            levelLoader.levelIndex = 0;
            SaveToFile();
        }
        
    }

    private void SaveToFile()
    {
        string path = Application.dataPath + "/save.txt";

        StreamWriter writer = new StreamWriter(path, false);
        writer.Write(unlockedChapter.val + "," + unlockedLevel.val);
        writer.Close();
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
        SaveToFile();
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

        if(unlockedChapter.val == currChap.Num && maxLevels > unlockedLevel.val)
        {
            maxLevels = unlockedLevel.val + 1;
        }

        for(int i = 0; i < maxLevels; i++)
        {
            LevelObject temp = Instantiate(levelPrefab, levelHolder);
            temp.SetLevelNum(i);
            temp.SetLevelSelect(this);
            temp.SetData(i + 1, currChap.Levels[i].Description);
        }

        chapterDisplay.refreshText();
    }

    public void LoadLevelsAnimated(bool isLeft)
    {
        StartCoroutine(AnimateLoadLevels(isLeft));
    }

    private IEnumerator AnimateLoadLevels(bool isLeft)
    {
        CanvasGroup levelHolderCG = levelHolder.GetComponent<CanvasGroup>();
        levelHolderCG.blocksRaycasts = false;

        float distance = levelHolder.GetComponent<RectTransform>().rect.width * 1.5f;
        if (!isLeft) distance *= -1;
        GameObject oldChapter = Instantiate(levelHolder.gameObject, levelHolder.parent);
        oldChapter.GetComponentInChildren<ChapterDisplay>().enabled = false;

        LoadLevels();

        levelHolderCG.alpha = 0;

        yield return new WaitForEndOfFrame();

        RectTransform newChapter = Instantiate(levelHolder.gameObject, levelHolder.parent).GetComponent<RectTransform>();
        //newChapter.anchoredPosition = new Vector2(newChapter.anchoredPosition.x + distance, newChapter.anchoredPosition.y);
        newChapter.anchoredPosition = new Vector2(newChapter.anchoredPosition.x + distance * -1, newChapter.anchoredPosition.y);
        newChapter.GetComponent<CanvasGroup>().alpha = 1;

        yield return new WaitForEndOfFrame();

        StartCoroutine(AnimateMove(oldChapter, distance));
        yield return new WaitForSeconds(0.02f);
        yield return StartCoroutine(AnimateMove(newChapter.gameObject, distance));

        levelHolderCG.alpha = 1;
        levelHolderCG.blocksRaycasts = true;
        Destroy(oldChapter);
        Destroy(newChapter.gameObject);
    }

    private IEnumerator AnimateMove(GameObject parent, float xPosition)
    {
        parent.GetComponent<ContentSizeFitter>().enabled = false;
        parent.GetComponent<LayoutGroup>().enabled = false;

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            LevelObject o = parent.transform.GetChild(i).GetComponent<LevelObject>();
            RectTransform rt = o.GetComponent<RectTransform>();
            o.MoveTo(new Vector2(rt.anchoredPosition.x + xPosition, rt.anchoredPosition.y));
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.5f);
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
        if (chapters.Chapters[levelLoader.chapterIndex].Levels[levelLoader.levelIndex].DialogueScript.text == null) yield break;
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
