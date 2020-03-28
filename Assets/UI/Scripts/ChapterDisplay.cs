using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChapterDisplay : MonoBehaviour {
	public string prefix = "Chapter ";
	public bool zeroIndexed = false;
	public LevelLoader levelLoader;
	public TextMeshProUGUI textMeshProUGUI;

	// Init
	private void Awake() {
		if (!textMeshProUGUI) {
			textMeshProUGUI = GetComponent<TextMeshProUGUI>();
		}
	}

	// Update is called once per frame
	void Update() {
		textMeshProUGUI.text = prefix + (levelLoader.chapterIndex + (zeroIndexed ? 0 : 1));
	}
}
