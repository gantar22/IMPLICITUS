using TypeUtil;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpellDrawerPopulator : MonoBehaviour {

	// ===== Fields =====
	public UnitEvent onLevelLoad;
	public LevelLoader levelLoader;

	private System.Action removeListener;


	// ===== Functions =====

	// Init
	private void Awake() {
		removeListener = onLevelLoad.AddRemovableListener(onLevelWasLoaded);
	}

	// After the level has loaded, populate the spell drawer with all the spells in the basis.
	private void onLevelWasLoaded(Unit obj) {
		foreach (Spell spell in levelLoader.currentLevel.Basis) {
			LayoutTracker newSpell = Instantiate(spell.prefab, transform);
			newSpell.enabled = false;
			newSpell.gameObject.AddComponent<LayoutElement>();
			newSpell.gameObject.AddComponent<DraggableSpell>().myCombinator = spell.combinator;
		}
	}

	private void OnDestroy() {
		removeListener();
	}
}
