using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterSelect : MonoBehaviour {
	public GameObject leftButton;
	public GameObject rightButton;
	public LevelLoader levelLoader;

	// Init
	void Awake() {
		refreshButtons();
	}

	// Update is called once per frame
	void Update() {
		refreshButtons();
	}

	// Enables/disables the left & right buttons according to the current chapter
	private void refreshButtons() {
		leftButton.SetActive(!levelLoader.atMinChapter());
		rightButton.SetActive(!levelLoader.atMaxChapter());
	}
}
