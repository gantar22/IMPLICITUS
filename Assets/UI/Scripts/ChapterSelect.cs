using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterSelect : MonoBehaviour {
	public GameObject leftButton;
	public GameObject rightButton;
	public LevelLoader levelLoader;
    public LevelSelect levelSelect;

	// Init
	void Awake() {
		refreshButtons();
	}

	// Enables/disables the left & right buttons according to the current chapter
	public void refreshButtons() {
		leftButton.SetActive(!levelLoader.atMinChapter());
		rightButton.SetActive(!levelLoader.atMaxChapter());
        levelSelect.LoadLevels();
	}
}
