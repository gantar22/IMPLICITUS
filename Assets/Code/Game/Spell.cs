using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Implicitus/New Spell")]
public class Spell : ScriptableObject
{
    [SerializeField]
    Combinator combinator;

    [SerializeField] private Sprite sprite;
    
}
