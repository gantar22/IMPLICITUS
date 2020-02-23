using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Implicitus/New Spell")]
public class Spell : ScriptableObject
{
    [SerializeField]
    public Combinator combinator;

    [SerializeField] public LayoutTracker prefab;
    
}
