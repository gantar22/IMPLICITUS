using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lambda;
using TypeUtil;
using System.Linq;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;

public class SpellCodexTab : MonoBehaviour {
	[SerializeField]
	private SymbolManagerTester startSMT;
	[SerializeField]
	private SymbolManagerTester targetSMT;

	public Shrub<Sum<Combinator, Variable>> startTerm;
	public Shrub<Sum<Combinator, Variable>> goalTerm;


	// Call this to set up the display 
	public void initialize(Combinator combinator) {
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
		for (int i=0; i < combinator.arity; i++) {
			termList.Add(Term.Leaf(Sum<Combinator, Variable>.Inr((Variable)i)));
		}
		startTerm = Term.Node(termList);
		startSMT.CreateTerm(startTerm);
		startSMT.Variables = tmpStartVariables;

		// Create Target
		targetSMT.Variables = split[0].Skip(1).Where(char.IsLetter).ToList();
		goalTerm = targetSMT.CreateTerm(split[1]);
		targetSMT.Variables = tmpTargetVariables;
	}
}
