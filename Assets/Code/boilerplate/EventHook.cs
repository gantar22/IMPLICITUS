using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventHook : MonoBehaviour
{
    [SerializeField] UnitEvent trigger;
    [SerializeField] UnityEvent listener;
    // Start is called before the first frame update
    void Start()
    {
        trigger.AddListener(listener.Invoke);
    }
    
}
