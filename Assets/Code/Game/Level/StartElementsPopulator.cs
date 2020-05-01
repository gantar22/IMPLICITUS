using System.Collections;
using System.Collections.Generic;
using TypeUtil;
using UnityEngine;
using UnityEngine.UI;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;

public class StartElementsPopulator : MonoBehaviour {

	// ===== Fields =====
	public IntEvent onLevelLoadArity;
	public SymbolManager symbolManager;
	private LayoutTracker oldVariables;
	public UnitEvent OnCastSpell;


	// ===== Functions =====

	// Init
	private void Awake() {
		onLevelLoadArity.AddRemovableListener(onLevelWasLoaded, this);
		if (OnCastSpell) {
			OnCastSpell.AddRemovableListener(_ => CastSpell(), this);
		}
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
			variables.Add(Term.Leaf(Sum<Combinator, Lambda.Variable>.Inr((Lambda.Variable)i)));
		}

		if (oldVariables)
		{
			Destroy(oldVariables.gameObject);		
		}
		oldVariables = symbolManager.Initialize(Term.Node(variables));

		// Set the alpha of the oldVariables parens (top level parens) to 0
		Image image = oldVariables.GetComponent<Image>();
		Color c = image.color;
		image.color = new Color(c.r, c.g, c.b, 0);
	}
}
