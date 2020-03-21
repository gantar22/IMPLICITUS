using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Implicitus/New Chapter")]
public class Chapter : ScriptableObject
{
    [SerializeField] private int _num;
    [SerializeField] private string _description;
    [SerializeField] private Level[] _levels;

    public int Num
    {
        get
        {
            return _num;
        }
    }

    public string Description
    {
        get
        {
            return _description;
        }
    }

    public Level[] Levels
    {
        get
        {
            return _levels;
        }
    }
}
