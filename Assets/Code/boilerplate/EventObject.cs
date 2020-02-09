using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventObject<T,TEventType> : ScriptableObject where TEventType : UnityEvent<T>, new()
{
    [SerializeField]
    TEventType Event;

    void Start()
    {
        Event = new TEventType();
    }

    public void AddListener(Action<T> f)
    {
        Event.AddListener(t => f(t));
    }

    public void AddListener<T2>(Func<T,T2> f)
    {
        Event.AddListener(t => f(t));
    }
    
    public void AddListenerOneTime(Action<T> f)
    {
        UnityAction<T> temp = null;
        temp = t =>
        {
            f(t);
            Event.RemoveListener(temp);
        };
        Event.AddListener(temp);
    }
    
    public void AddListenerOneTime<T2>(Func<T,T2> f)
    {
        UnityAction<T> temp = null;
        temp = t =>
        {
            f(t);
            Event.RemoveListener(temp);
        };
        Event.AddListener(temp);
    }

    public void Invoke(T t)
    {
        Event.Invoke(t);
    }

    public void InvokeAsync(MonoBehaviour m, T t)
    {
        
        IEnumerator Invoke()
        {
            yield return null;
            
            Event.Invoke(t);
        }
        m.StartCoroutine(Invoke());
    }
}