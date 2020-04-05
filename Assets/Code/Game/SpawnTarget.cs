using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lambda;
using TypeUtil;
using UnityEngine;

public class SpawnTarget : MonoBehaviour
{
    [SerializeField] private StringEvent lambda;

    [SerializeField] private SymbolManagerTester smt;

    public Shrub<Sum<Combinator, Variable>> goal;

    private Action onDestroy;
    // Start is called before the first frame update
    void Awake()
    {
        onDestroy = lambda.AddRemovableListener(createTarget);
    }

    private void Start()
    {
        smt.GetComponent<SymbolManager>().onCreateTerm.Add(t => goal = t);
    }

    void createTarget(string s)
    {
        print("spawn target");
        char[] c = {'>'};
        var split = s.Split(c);
        var tmp = smt.Variables;
        smt.Variables = split[0].Skip(1).Where(char.IsLetter).ToList();
        goal = smt.CreateTerm(split[1]);
        smt.Variables = tmp;
    }

    public void createTarget(Shrub<Sum<Combinator, Variable>> t)
    {
        goal = t;
        smt.CreateTerm(goal);
    }

    private void OnDestroy()
    {
        onDestroy();
    }

    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
