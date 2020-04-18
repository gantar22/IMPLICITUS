using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CodexOnSpell : MonoBehaviour, IPointerClickHandler {
	// Fields
	public SpellCodexTab SpellCodexTabPrefab;

	// Private vars
	private DraggableSpell draggableSpell;
	private SpellCodexTab currentCodexTab;


	// Init
	private void Awake() {
		draggableSpell = GetComponent<DraggableSpell>();
	}

	// Pointer click listener to toggle the CodexTab whenever this is clicked.
	public void OnPointerClick(PointerEventData eventData) {
		if (draggableSpell.hasBeenDragged) {
			draggableSpell.OnEndDrag(null);
		} else {
			toggleCodexTab();
		}
	}

	// If a codexTab exists, delete it. Otherwise, create it.
	public void toggleCodexTab() {
		if (currentCodexTab && currentCodexTab.gameObject) {
			deleteCodexTab();
		} else {
			createCodexTab();
		}
	}

	// Create a new codexTab for this spell.
	public void createCodexTab() {
		// TODO - uncomment these and implement the Combinator display on the Spell Codex Tab prefab.
		//   Currently disabled so that it doesn't mess with stuff while it's still in progress
		//currentCodexTab = Instantiate(SpellCodexTabPrefab, transform.parent);
		//currentCodexTab.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
	}

	// Delete the current codextTab for this spell (if it exists).
	public void deleteCodexTab() {
		if (currentCodexTab && currentCodexTab.gameObject) {
			Destroy(currentCodexTab.gameObject);
		}
	}
}
