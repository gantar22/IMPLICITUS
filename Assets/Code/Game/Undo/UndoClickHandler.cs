using System.Collections;
using System.Collections.Generic;
using Lambda;
using TypeUtil;
using UnityEngine;


[RequireComponent(typeof(UndoManager))]
public class UndoClickHandler : MonoBehaviour, TermClickHandler {
	// Fields
	private UndoManager undoManager;


	// Init
	private void Awake() {
		undoManager = GetComponent<UndoManager>();
	}

	// When a term is clicked
	public void HandleClick(SymbolManager symbolManager, Shrub<Sum<Combinator, Variable>> term, List<int> path, LayoutTracker root) {
		undoManager.undo();
	}
}