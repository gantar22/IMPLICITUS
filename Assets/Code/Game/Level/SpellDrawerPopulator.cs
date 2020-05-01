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
	public BoolRef noParens;


	// ===== Functions =====

	// Init
	private void Awake() {
		onLevelLoad.AddRemovableListener(onLevelWasLoaded, this);
	}

	// After the level has loaded, populate the spell drawer with all the spells in the basis.
	private void onLevelWasLoaded(Unit obj) {
		foreach (Spell spell in levelLoader.currentLevel.Basis) {
			spawnSpell(spell.prefab,spell.combinator);
		}
		if(!noParens.val)
			spawnSpell(parenPrefab,null); //null represents parens :(
	}

	private void spawnSpell(LayoutTracker prefab, Combinator combinator)
	{
		LayoutTracker newSpell = Instantiate(prefab, transform);
		newSpell.enabled = false;
		newSpell.gameObject.AddComponent<LayoutElement>();

	}
}
