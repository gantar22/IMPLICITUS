using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newLevelObject", menuName = "LevelObject")]
public class LevelObject : ScriptableObject
{

    /* Data Field */

    //Name of Level
    [SerializeField] private string levelName;

    //Level Description; Narrative or pratical purposes
    [SerializeField] private string description;

    //List Of Combinators in Level
    [SerializeField] private List<Combinator> combinators;

    //Spell being solved for
    [SerializeField] private Spell goalSpell;

    /* Get Functions */

    public string LevelName { get {return levelName; } }
    public string Description { get { return description; } }
       
}
