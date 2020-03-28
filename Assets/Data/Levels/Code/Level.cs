using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Implicitus/New Level")]
public class Level : ScriptableObject
{
    [SerializeField] private int _num;
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private Spell[] _basis;
    [SerializeField] private Combinator _goal;

    public int Num
    {
        get
        {
            return _num;
        }
    }

    public string Name
    {
        get
        {
            return _name;
        }
    }

    public string Description
    {
        get
        {
            return _description;
        }
    }

    public Spell[] Basis
    {
        get
        {
            return _basis;
        }
    }

    public Combinator Goal
    {
        get
        {
            return _goal;
        }
    }

}
