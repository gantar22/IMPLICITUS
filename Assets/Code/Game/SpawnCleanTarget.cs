using Lambda;
using System.Linq;
using TypeUtil;
using UnityEngine;

public class SpawnCleanTarget : MonoBehaviour {
	[SerializeField] private StringEvent lambda;

	[SerializeField] private SymbolManagerTester smt;

	public Shrub<Sum<Combinator, Variable>> goal;


	// Start is called before the first frame update
	void Awake() {
		lambda.AddRemovableListener(createTarget, this);
	}

	void createTarget(string s) {
		char[] c = { '>' };
		var split = s.Split(c);
		var tmp = smt.Variables;
		smt.Variables = split[0].Skip(1).Where(char.IsLetter).ToList();
		goal = smt.CreateTerm(split[1]);
		smt.Variables = tmp;
	}
}
