using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Reference<T,EventType> : ScriptableObject where EventType : UnityEvent<T>, new()
{
    [SerializeField] private T value;
    public T val
    {
        get { return value;}
        set
        {
            if (OnChange == null)
                OnChange = new EventType();
            OnChange.Invoke(value);
            this.value = value;
        }
    }

    private T prev_t;


    public EventType OnChange;

}