using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryLoader : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private ChapterList chapters;
    [SerializeField] private LevelLoader levelLoader;
#pragma warning restore 0649

    private Coroutine routine;

    private void Start()
    {
        if(levelLoader.story)
        {
            LoadStory();
            levelLoader.story = false;
        }
    }

    public void LoadStory()
    {
        if (routine == null) routine = StartCoroutine(LoadStoryScene());
    }

    private IEnumerator LoadStoryScene()
    {
        LoadManager.instance.LoadSceneAsync("Dialogue");
        while (DialogueManager.instance == null)
        {
            yield return new WaitForEndOfFrame();
        }

        var dialogue = chapters.Chapters[levelLoader.chapterIndex].Levels[levelLoader.levelIndex].DialogueScript;
        if(dialogue)
            DialogueManager.instance.PlayDialogue(dialogue.text);
        else
            DialogueManager.instance.PlayDialogue(" ");
        routine = null;
    }
}
