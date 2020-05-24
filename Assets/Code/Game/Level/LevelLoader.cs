using System.Collections.Generic;
using System.Linq;
using TypeUtil;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;


[CreateAssetMenu(menuName ="levelLoader")]
public class LevelLoader : ScriptableObject {

	// ===== Fields =====
	[SerializeField] private ChapterList chapterList;
	[SerializeField] private int levelSceneBuildIndex;
	[SerializeField] private Level levelRandom;
	[SerializeField] private SpellList allSpells;
	[SerializeField] private SpellList levelRandomExclusions;

	[SerializeField] private UnitEvent onLevelLoad;
	[SerializeField] private StringEvent onLevelLoadLambda;
	[SerializeField] private IntEvent onLevelLoadArity;

	[SerializeField] private BoolRef NoParens;
	[SerializeField] private BoolRef NoBackApp;
	[SerializeField] private BoolRef NoForwardApp;
	
	[HideInInspector] public int chapterIndex = -1;
	[HideInInspector] public int levelIndex = -1;
	[HideInInspector] public Level currentLevel;
    [HideInInspector] public bool story = false;

	public static bool isLevelRandom = false;

	[SerializeField] IntEvent effectAudioEvent; //Event Calls audio sound
	[SerializeField] IntEvent songAudioEvent; //Event Calls audio sounds

	//Debug event
	[System.Serializable]
	private struct EventHolder { public UnityEvent e; }
	[SerializeField] private EventHolder debugEvent;
	private static LevelLoader instance;

	// ===== Setters/Getters =====
	public void setChapterIndex(int newIndex) {
		isLevelRandom = false;
		chapterIndex = newIndex;
		verifyChapterIndex();
	}
	public void setLevelIndex(int newIndex) {
		isLevelRandom = false;
		levelIndex = newIndex;
	}
	public void decrChapterIndex() {
		isLevelRandom = false;
		chapterIndex--;
		verifyChapterIndex();
	}
	public void incrChapterIndex() {
		isLevelRandom = false;
		chapterIndex++;
		verifyChapterIndex();
	}


	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			isLevelRandom = false;
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
		if (isLevelRandom) {
			startLevelRandom();
			return;
		}
		loadLevel(chapterIndex, levelIndex);
	}

	// Loads a particular level by chapter and level indices
	public void loadLevel(int chapter, int level) {
		// WARNING - changes made here might need to be reflected in startLevelRandom() as well.
		songAudioEvent.Invoke(3); //Puzzle Track
		isLevelRandom = false;

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

    //Loads the next level and the accompanied story
    public void loadNextLevelStory() {
		if (isLevelRandom) {
			startLevelRandom();
			return;
		}
		story = true;
        loadNextLevel();
    }



    // Once a level has been loaded, calls all the initialization events
    private System.Action<AsyncOperation> onLevelLoaded(Level levelLoaded)
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
#if UNITY_EDITOR
	[MenuItem("Assets/IMPLICITUS/LoadSpecificLevel")]
#endif
	public static void debugTrigger()
	{
		instance.debugEvent.e.Invoke();
	}


	// ===== levelRandom functionality =====
	public void startLevelRandom() {
		isLevelRandom = true;
		rerandomizeLevelRandom();

		songAudioEvent.Invoke(3); //Puzzle Track
		currentLevel = levelRandom;
		SceneManager.LoadSceneAsync(levelSceneBuildIndex).completed += onLevelLoaded(currentLevel);
	}

	// Randomize the levelRandom object to a new random state
	private void rerandomizeLevelRandom() {
		List<Spell> allUsableSpells = allSpells.spells.ConvertAll(s => s);
		allUsableSpells.RemoveAll(spell => levelRandomExclusions.spells.Contains(spell));
		int usableSpellsCount = allUsableSpells.Count;

		// ---- Settings ----
		int numCombinatorsToApply = Random.Range(2, 5); // How many combinators will be part of the solution (inclusive, exclusive).
		int numAddedToBasis = Random.Range(0, 2); // How many additional (random) spells will be added to the basis.
		float doubleCombinatorOdds = 0.4f; // For each combinator, the odds of it getting applied twice somewhere in the solution.
		const int EVALUATION_LIMIT = 15; // Limit on the number of times a term can be evaluated, to detect (likely) infinite loops.
		const int MAX_VARIABLES = 5; // Limit on the the number of variables that can be in the term.
		const int MAX_COMBINATORS = 4; // Limit on the number of combinators that can go into the basis at all (because more won't fit well into the left panel).

		// Make sure not to add an excessive number of combinators to the basis
		while (numAddedToBasis + numCombinatorsToApply > MAX_COMBINATORS) {
			numAddedToBasis--;
		}

		// ---- Term Generation ----
		// Choose the random spells and start constructing the Term leafList
		List<Spell> spellsToApply = new List<Spell>();
		List<Term> baseLeafList = new List<Term>();
		for (int i = 0; i < numCombinatorsToApply; i++) {
			Spell randSpell = allUsableSpells[Random.Range(0, usableSpellsCount)];
			spellsToApply.Add(randSpell);
			baseLeafList.Add(Term.Leaf(Sum<Combinator, Lambda.Variable>.Inl(randSpell.combinator)));
			// Some odds to add this combinator twice
			if (Random.Range(0f, 1f) < doubleCombinatorOdds) {
				baseLeafList.Add(Term.Leaf(Sum<Combinator, Lambda.Variable>.Inl(randSpell.combinator)));
			}
		}

		// Shuffle
		for (int i = 0; i < baseLeafList.Count - 1; i++) {
			int toSwap = Random.Range(i, baseLeafList.Count);
			Term tempLeaf = baseLeafList[toSwap];
			baseLeafList[toSwap] = baseLeafList[i];
			baseLeafList[i] = tempLeaf;
		}

		// Test increasingly large number of variables
		for (int numVariables = 1; numVariables <= MAX_VARIABLES; numVariables++) {
			List<Term> leafList = new List<Term>(baseLeafList);
			// Add the variables to the leafList
			for (int i = 0; i < numVariables; i++) {
				leafList.Add(Term.Leaf(Sum<Combinator, Lambda.Variable>.Inr((Lambda.Variable)i)));
			}

			// Construct the Term node from the leafList
			Term term = Term.Node(leafList);

			int evaluationCount = 0;
			List<Lambda.ElimRule> elimRules = Lambda.Util.CanEvaluate(term, new List<int>(), (v, rule) => rule);
			while (elimRules.Count > 0 && evaluationCount < EVALUATION_LIMIT) {
				evaluationCount++;
				term = elimRules[0].evaluate(term);
				elimRules = Lambda.Util.CanEvaluate(term, new List<int>(), (v, rule) => rule);
			}

			// Have to check if it could successfully (and fully) evaluate. Otherwise retry
			if (evaluationCount >= EVALUATION_LIMIT || containsCombinator(term)) {
				// If it fails, try an additional variable (or reach the limit and try a completely new term)
				continue;
			}
			// If it succeeds, complete the generation

			// ---- Lambda Term ----
			// Generate the first half of the lambdaTerm (string)
			levelRandom.Goal.lambdaTerm = "R ";
			for (int i = 0; i < numVariables; i++) {
				levelRandom.Goal.lambdaTerm += (char)(122 - i) + " ";
			}
			levelRandom.Goal.lambdaTerm += "=> ";

			// Generate the second half of the lambdaTerm (See Shrub.toString() as reference)
			string resultLambdaString = string.Join(" ", term.MapPreorder(
				t => t.Match(combin => combin.ToString(), variab => variab.ToString()),
				l => l.Prepend("(").Append(")").ToList()
				));

			// Convert the variable ints to variables
			for (int i = 0; i < numVariables; i++) {
				resultLambdaString = resultLambdaString.Replace(i.ToString().ToCharArray()[0], (char)(122 - i));
			}

			// Trim off the first and last parens
			resultLambdaString = resultLambdaString.Remove(0, 1);
			resultLambdaString = resultLambdaString.Remove(resultLambdaString.Length - 1, 1);

			// Finally, append it to the lambdaTerm (string)
			levelRandom.Goal.lambdaTerm += resultLambdaString;

			// ---- Basis ----
			// Add numAddedToBasis random combinators to the list
			for (int i = 0; i < numAddedToBasis; i++) {
				Spell randSpell = allUsableSpells[Random.Range(0, usableSpellsCount)];
				spellsToApply.Insert(Random.Range(0, spellsToApply.Count), randSpell);
			}
			// Deduplicate
			for (int i = 0; i < spellsToApply.Count; i++) {
				for (int j = i + 1; j < spellsToApply.Count; j++) {
					if (spellsToApply[i] == spellsToApply[j]) {
						spellsToApply.RemoveAt(j);
						i = 0;
						j = 0;
					}
				}
			}
			// Shuffle
			for (int i = 0; i < spellsToApply.Count - 1; i++) {
				int toSwap = Random.Range(i, spellsToApply.Count);
				Spell tempSpell = spellsToApply[toSwap];
				spellsToApply[toSwap] = spellsToApply[i];
				spellsToApply[i] = tempSpell;
			}
			levelRandom.secretlySetBasis(spellsToApply.ToArray());
			return;
		}
		rerandomizeLevelRandom();
	}


	// Helper function for a Term that returns true iff the shrub contains at least one combinator
	public bool containsCombinator(Term term) {
		return term.Match(
				l => l.Exists(t2 => containsCombinator(t2)),
				v => v.Match(
					comb => true,
					variable => false
					));
		}
}
