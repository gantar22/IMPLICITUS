using System;
using System.Collections;
using System.Collections.Generic;
using TypeUtil;
using UnityEngine;
using UnityEngine.Events;

public class EventRetrigger<T,TEventObject,TEvent> : MonoBehaviour where TEvent : UnityEvent<T>, new() where TEventObject : EventObject<T,TEvent> 
{
    [SerializeField] TEventObject e;
    private Sum<T,Unit> args = Sum<T, Unit>.Inr(new Unit());
    private Action cleanup;
    // Start is called before the first frame update
    void Awake()
    {
        e.AddRemovableListener(x => args = Sum<T,Unit>.Inl(x),this);
    }

    public void ReTrigger()
    {
        args.Match(e.Invoke, _ => { });
    }
}