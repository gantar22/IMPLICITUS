using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;


public class Undoable : MonoBehaviour {
	// Public fields
	public TermEvent undoTermEvent;
	public SymbolManagerTester symbolManagerTester;


	// init
	private void Awake() {
		undoTermEvent.AddRemovableListener(undoToTerm, this);
	}

	// When the undo term event triggers, update to the new term
	private void undoToTerm(Term term) {
		symbolManagerTester.CreateTerm(term);
	}
}
