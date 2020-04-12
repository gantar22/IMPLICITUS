using System;
using System.Collections;
using System.Collections.Generic;
using TypeUtil;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName ="levelLoader")]
public class LevelLoader : ScriptableObject {

	// ===== Fields =====
	[SerializeField] private ChapterList chapterList;
	[SerializeField] private int levelSceneBuildIndex;

	[SerializeField] private UnitEvent onLevelLoad;
	[SerializeField] private StringEvent onLevelLoadLambda;
	[SerializeField] private IntEvent onLevelLoadArity;

	[SerializeField] private BoolRef NoParens;
	[SerializeField] private BoolRef NoBackApp;
	[SerializeField] private BoolRef NoForwardApp;
	
	[HideInInspector] public int chapterIndex = -1;
	[HideInInspector] public int levelIndex = -1;
	[HideInInspector] public Level currentLevel;

	//Debug event
	[System.Serializable]
	private struct EventHolder { public UnityEvent e; }
	[SerializeField] private EventHolder debugEvent;
	private static LevelLoader instance;

	// ===== Setters/Getters =====
	public void setChapterIndex(int newIndex) {
		chapterIndex = newIndex;
		verifyChapterIndex();
	}
	public void setLevelIndex(int newIndex) {
		levelIndex = newIndex;
	}
	public void decrChapterIndex() {
		chapterIndex--;
		verifyChapterIndex();
	}
	public void incrChapterIndex() {
		chapterIndex++;
		verifyChapterIndex();
	}

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
		}
		else
		{
			name = name + " does not receive debug events";
		}
	}

	private void OnEnable()
	{
		if (!instance) instance = this;
	}


	// Ensures 0 <= chapterIndex < chapterList.Chapters.Length
	private void verifyChapterIndex() {
		if (chapterIndex < 0) {
			Debug.LogError("Tried to set chapterIndex below 0...");
			chapterIndex = 0;
		}
		if (chapterList.Chapters.Length <= chapterIndex) {
			Debug.LogError("Tried to setChapterIndex too high... chapter " + chapterIndex + " (zero-indexed) does not exist.");
			chapterIndex = chapterList.Chapters.Length - 1;
		}
	}

	// Returns true iff chapterIndex is the minimum/maximum chapter, respectively
	public bool atMinChapter() {
		return chapterIndex == 0;
	}
	public bool atMaxChapter() {
		return chapterIndex == chapterList.Chapters.Length - 1;
	}


	// ===== Functions =====

	// Loads the next level in the sequence of levels
	public void loadNextLevel() {
		if (!currentLevel || chapterIndex < 0 || levelIndex < 0) {
			Debug.LogWarning("loadNextLevel() called while currentLevel not set. Defaulting to first level.");
			loadLevel(0, 0);
			return;
		}

		if (chapterList.Chapters[chapterIndex].Levels.Length <= levelIndex + 1) {
			Debug.LogWarning($"loadNextLevel() called from last level in chapter {chapterIndex}. Wrapping around to first level of next chapter.");
			loadLevel(chapterIndex + 1, 0);
		} else {
			loadLevel(chapterIndex, levelIndex + 1);
		}
	}

	// Loads the level according to the current chapterIndex and levelIndex
	public void loadSelectedLevel() {
		loadLevel(chapterIndex, levelIndex);
	}

	// Loads a particular level by chapter and level indices
	public void loadLevel(int chapter, int level) {
		if (chapter >= chapterList.Chapters.Length || chapter < 0 || level >= chapterList.Chapters[chapter].Levels.Length || level < 0) {
			Debug.LogError($"Trying to load chapter {chapter}, level {level}, which does not exist. Defaulting to chapter 0 level 0.");
			loadLevel(0, 0);
			return;
		}
		chapterIndex = chapter;
		levelIndex = level;
		currentLevel = chapterList.Chapters[chapterIndex].Levels[levelIndex];
		SceneManager.LoadSceneAsync(levelSceneBuildIndex).completed += onLevelLoaded(currentLevel);
	}

	
	
	// Once a level has been loaded, calls all the initialization events
	private Action<AsyncOperation> onLevelLoaded(Level levelLoaded)
	{ //Curried to ensure that the loaded level is the one which was intended
		return _ =>
		{
			NoParens.val = levelLoaded.Restrictions.noParens;
			NoBackApp.val = levelLoaded.Restrictions.noBackApp;
			NoForwardApp.val = levelLoaded.Restrictions.noForwardApp;
			
			onLevelLoad.InvokeAsync(new Unit());
			onLevelLoadArity.InvokeAsync(levelLoaded.Goal.arity);
			onLevelLoadLambda.InvokeAsync(levelLoaded.Goal.lambdaTerm);
		};
	}
	
	// Hook to trigger debug event, intended for jumping to specific levels
	[MenuItem("Assets/IMPLICITUS/LoadSpecificLevel")]
	public static void debugTrigger()
	{
		instance.debugEvent.e.Invoke();
	}
}
