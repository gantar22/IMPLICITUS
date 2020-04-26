using System.Collections.Generic;
using UnityEngine;
using Lambda;
using TypeUtil;
using System.Linq;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;
using TMPro;
using UnityEngine.UI;

public class Codex : MonoBehaviour {
	[SerializeField]
	private TextMeshProUGUI nameTMP;
	[SerializeField]
	private SymbolManagerTester startSMT;
	[SerializeField]
	private SymbolManagerTester targetSMT;
	[SerializeField]
	private Image arrowImage;

	public CombinatorEvent updateCodexEvent;

	public Shrub<Sum<Combinator, Variable>> startTerm;
	public Shrub<Sum<Combinator, Variable>> goalTerm;

	private Combinator currentCombinator = null;


	// Init
	private void Awake() {
		updateCodexEvent.AddRemovableListener(initialize, this);
	}

	// Call this to set up the display 
	public void initialize(Combinator combinator) {
		if (combinator == currentCombinator) {
			return;
		}
		currentCombinator = combinator;
		arrowImage.enabled = true;

		nameTMP.text = combinator.info.nameInfo.spellName;

		string lambda = combinator.lambdaTerm;
		// From SpawnTarget:createTarget()
		char[] c = { '>' };
		var split = lambda.Split(c);
		var tmpStartVariables = startSMT.Variables;
		var tmpTargetVariables = targetSMT.Variables;

		// Create Start (proposal)
		// Construct the term using the combinator and then arity variables
		startSMT.Variables = split[0].Skip(1).Where(char.IsLetter).ToList();
		List<Term> termList = new List<Term>();
		termList.Add(Term.Leaf(Sum<Combinator, Variable>.Inl(combinator)));
		for (int i = 0; i < combinator.arity; i++) {
			termList.Add(Term.Leaf(Sum<Combinator, Variable>.Inr((Variable)i)));
		}
		startTerm = Term.Node(termList);
		startSMT.reset();
		startSMT.CreateTerm(startTerm);
		startSMT.Variables = tmpStartVariables;

		// Create Target
		//targetSMT.reset();
		// TODO - use ParseCombinator, or find another way, so that a new parens aren't created each time, or so you can destroy the previous ones
		//Util.ParseCombinator(combinator);
		targetSMT.Variables = split[0].Skip(1).Where(char.IsLetter).ToList();
		goalTerm = targetSMT.CreateTerm(split[1]);
		targetSMT.CreateTerm(goalTerm);
		targetSMT.Variables = tmpTargetVariables;
		targetSMT.currentLayout.GetComponent<Image>().color = Color.clear;
	}
}