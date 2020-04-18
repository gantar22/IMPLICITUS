using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TypeUtil;
using UnityEngine;
using UnityEngine.UI;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;

public class StartElementsPopulator : MonoBehaviour {

	// ===== Fields =====
	public IntEvent onLevelLoadArity;
	public SymbolManager symbolManager;
	private System.Action removeListener;
	private LayoutTracker oldVariables;
	public UnitEvent OnCastSpell;


	// ===== Functions =====

	// Init
	private void Awake() {
		removeListener = onLevelLoadArity.AddRemovableListener(onLevelWasLoaded);
		OnCastSpell.AddRemovableListener(_ => CastSpell(), this);
	}

	private void CastSpell()
	{
		if(oldVariables)
			oldVariables.GetComponent<Image>().color = Color.clear;
	}
	
	// After the level has loaded, populate the skeleton using the symbol manager
	private void onLevelWasLoaded(int arity) {
		
		List<Term> variables = new List<Term>();
		for (int i=0; i < arity; i++) {
			variables.Add(Term.Leaf(TypeUtil.Sum<Combinator, Lambda.Variable>.Inr((Lambda.Variable)i)));
		}
		if(oldVariables)
			Destroy(oldVariables.gameObject);
		oldVariables = symbolManager.Initialize(Term.Node(variables));

    }

	// When this is destroyed, remove listener.
	private void OnDestroy() {
		removeListener();
	}
}
