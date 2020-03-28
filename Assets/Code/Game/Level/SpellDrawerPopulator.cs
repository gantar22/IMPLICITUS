using TypeUtil;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpellDrawerPopulator : MonoBehaviour {

	// ===== Fields =====
	public UnitEvent onLevelLoad;
	public LevelLoader levelLoader;
	public UnitEvent onApplyProposal;
	public UnitEvent onUnapplyProposal;
	public LayoutTracker parenPrefab;
	
	private System.Action removeListener;


	// ===== Functions =====

	// Init
	private void Awake() {
		removeListener = onLevelLoad.AddRemovableListener(onLevelWasLoaded);
	}

	// After the level has loaded, populate the spell drawer with all the spells in the basis.
	private void onLevelWasLoaded(Unit obj) {
		foreach (Spell spell in levelLoader.currentLevel.Basis) {
			spawnSpell(spell.prefab,spell.combinator);
		}
		spawnSpell(parenPrefab,null); //null represents parens :(
	}

	private void spawnSpell(LayoutTracker prefab, Combinator combinator)
	{
		LayoutTracker newSpell = Instantiate(prefab, transform);
		newSpell.enabled = false;
		newSpell.gameObject.AddComponent<LayoutElement>();
		DraggableSpell draggable = newSpell.gameObject.AddComponent<DraggableSpell>();
		draggable.myCombinator = combinator;
		draggable.onApply = onApplyProposal;
		draggable.onUnapply = onUnapplyProposal;
	}
	
	private void OnDestroy() {
		removeListener();
	}
}
