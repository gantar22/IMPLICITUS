using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


// MOSTLY DEPRECATED (but still being used to update the *new* codex)

public class CodexOnSpell : MonoBehaviour, IPointerClickHandler {
	// Fields
	public SpellCodexTab SpellCodexTabPrefab;
	public CombinatorEvent updateCodexEvent;

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
			//toggleCodexTab();
			if (updateCodexEvent && draggableSpell) {
				updateCodexEvent.Invoke(draggableSpell.myCombinator);
			}
		}
	}

	// If a codexTab exists, delete it. Otherwise, create it.
	public void toggleCodexTab() {
		//if (currentCodexTab && currentCodexTab.gameObject) {
		//	deleteCodexTab();
		//} else {
		//	createCodexTab();
		//}
	}

	// Create a new codexTab for this spell.
	public void createCodexTab() {
		// Only create a codex tab within the left pane
		//DraggableHolder draggableHolder = GetComponentInParent<DraggableHolder>();
		//if (draggableHolder && draggableHolder.myType == DraggableHolder.DraggableType.LeftPane) {
		//	if (SpellCodexTabPrefab && transform && transform.parent) {
		//		currentCodexTab = Instantiate(SpellCodexTabPrefab, transform.parent);
		//		currentCodexTab.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
		//		currentCodexTab.initialize(draggableSpell.myCombinator);
		//	}
		//}
	}

	// Delete the current codextTab for this spell (if it exists).
	public void deleteCodexTab() {
		if (currentCodexTab && currentCodexTab.gameObject) {
			Destroy(currentCodexTab.gameObject);
		}
	}
}
