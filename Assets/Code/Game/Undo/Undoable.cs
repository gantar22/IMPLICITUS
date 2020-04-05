using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;


public class Undoable : MonoBehaviour {
	// Public fields
	public TermEvent undoTermEvent;
	public SymbolManagerTester symbolManagerTester;

	// Private fields
	private System.Action removeUndoToTermListener;


	// init
	private void Awake() {
		removeUndoToTermListener = undoTermEvent.AddRemovableListener(undoToTerm);
	}

	// When the undo term event triggers, update to the new term
	private void undoToTerm(Term term) {
		symbolManagerTester.CreateTerm(term);
	}

	// When this is destroyed, remove itself as a listener
	private void OnDestroy() {
		removeUndoToTermListener();
	}
}
