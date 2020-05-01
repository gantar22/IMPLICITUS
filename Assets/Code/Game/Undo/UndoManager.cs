using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;


[RequireComponent(typeof(SymbolManager))]
public class UndoManager : MonoBehaviour {
	public TermEvent pushUndoTermEvent;
	public TermEvent undoTermEvent;

	// Private fields
	private SymbolManager symbolManager;
	private LayoutTracker currentLayoutTracker;
	private Stack<Term> stack = new Stack<Term>();


	// Init
	private void Awake() {
		symbolManager = GetComponent<SymbolManager>();
		pushUndoTermEvent.AddRemovableListener(pushTerm, this);
	}

	// Push a new term onto the undo stack
	public void pushTerm(Term term) {
		stack.Push(term);
		if (currentLayoutTracker) {
			Destroy(currentLayoutTracker.gameObject);
		}
		currentLayoutTracker = symbolManager.Initialize(term);
	}

	// Call this to pop the undo stack onto the goal
	public void undo() {
		// Invoke the undoTermEvent with the top term, so that the real goal symbol manager can update itself
		if (stack.Count < 1) {
			Debug.LogWarning("Tried to undo when there's nothing on the undo stack. Ignoring it.");
			return;
		}
		undoTermEvent.Invoke(stack.Pop());

		// Delete the current term
		if (currentLayoutTracker) {
			Destroy(currentLayoutTracker.gameObject);
			currentLayoutTracker = null;
		}

		// If there's no next undo term, that's fine
		if (stack.Count < 1) {
			return;
		}

		// Place the next undo term
		Term nextUndoTerm = stack.Peek();
		currentLayoutTracker = symbolManager.Initialize(nextUndoTerm);
	}
}
