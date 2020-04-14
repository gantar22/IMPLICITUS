using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] IntEvent effectAudioEvent; //Event Calls audio sound
    [SerializeField] IntEvent songAudioEvent; //Event Calls audio sounds


    //List of listeners (to know when to play sounds)
    [SerializeField] UnitEvent completedPuzzle;

    //Will listen for events to invoke correct sounds!
    void Awake()
    {
        //Empty for now, may implement if more useful later
    }

}
