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
        s = s.Split(c)[1];
        goal = smt.CreateTerm(s);
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
