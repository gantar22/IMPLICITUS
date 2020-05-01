using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterSelect : MonoBehaviour {
	public Button leftButton;
	public Button rightButton;
	public LevelLoader levelLoader;
    public LevelSelect levelSelect;
#pragma warning disable 0649
    [SerializeField] private IntRef unlockedChapter;
#pragma warning restore 0649

    // Init
    void Awake() {
		refreshButtons();
	}

	// Enables/disables the left & right buttons according to the current chapter
	public void refreshButtons() {
		leftButton.interactable = !levelLoader.atMinChapter();
		rightButton.interactable = !levelLoader.atMaxChapter() && levelLoader.chapterIndex < unlockedChapter.val;
        //levelSelect.LoadLevels();
	}
}
