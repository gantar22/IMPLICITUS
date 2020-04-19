using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterSelect : MonoBehaviour {
	public Button leftButton;
	public Button rightButton;
	public LevelLoader levelLoader;
    public LevelSelect levelSelect;

	// Init
	void Awake() {
		refreshButtons();
	}

	// Enables/disables the left & right buttons according to the current chapter
	public void refreshButtons() {
		leftButton.interactable = !levelLoader.atMinChapter();
		rightButton.interactable = !levelLoader.atMaxChapter();
        levelSelect.LoadLevels();
	}
}
