using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public struct CombinatorInfo
{
    public bool duplicator;
    public bool associator;
    public bool permutor;
    public bool recursive;
    public CombinatorNameInfo nameInfo;
    [TextArea(3,10)] public string extraInfo;
}

[System.Serializable]
public struct CombinatorNameInfo
{
    public char name;
    public string birdName;
    public string spellName;
    public string logicName;
}

[CreateAssetMenu(menuName = "Implicitus/New Combinator")]
public class Combinator : ScriptableObject
{
    [SerializeField] public string lambdaTerm;
    [SerializeField] public CombinatorInfo info;

    private string _currentTerm;
    private int _arity = -1;
    
    
    public int arity
    {
        get
        {
            if (_currentTerm != lambdaTerm)
            {
                _currentTerm = lambdaTerm;
                _arity = Lambda.Util.ParseCombinator(this).Match(pi => pi.Item2, u => -1);
            }
            return _arity;
        }
    }

    public override string ToString()
    {
        return info.nameInfo.name.ToString();
    }
}
