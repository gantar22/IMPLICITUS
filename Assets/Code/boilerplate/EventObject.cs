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
        AddListenerOneTime(t => { f(t); });
    }

	public Action AddRemovableListener(Action<T> f) {
		UnityAction<T> temp = null;
		temp = t => f(t);
		Event.AddListener(temp);
		return () => Event.RemoveListener(temp);
	}

	public Action AddRemovableListener<T2>(Func<T, T2> f)
    {
        return AddRemovableListener(t => { f(t); });
    }
	
    public void AddRemovableListener(Action<T> f,MonoBehaviour responsible)
    {
        var cleanup = AddRemovableListener(f);

        IEnumerator tillDestroy()
        {
            void Cleanup() => cleanup();
            OnDestroyTrigger t = null;
            
            if (responsible.gameObject.GetComponent<OnDestroyTrigger>())
                t = responsible.gameObject.GetComponent<OnDestroyTrigger>();
            else 
                t = responsible.gameObject.AddComponent<OnDestroyTrigger>();
            
            t.e.AddListener(Cleanup);
            
            yield return new WaitUntil(() => responsible == null);
            
            t.e.RemoveListener(Cleanup);
            cleanup();
        }

        responsible.StartCoroutine(tillDestroy());
    }

    public void AddRemovableListener(Action<T> f, GameObject responsible)
    {
        var cleanup = AddRemovableListener(f);
        responsible.AddComponent<OnDestroyTrigger>().e.AddListener(() => cleanup());
    }
    
    public void AddRemovableListener<T2>(Func<T,T2> f,MonoBehaviour responsible)
    {
        AddRemovableListener(t => { f(t); }, responsible);
    }

	private void RemoveListener(UnityAction<T> listener) {
		Event.RemoveListener(listener);
	}

	public void Invoke(T t)
    {
        Event.Invoke(t);
    }
    
    public void InvokeAsync(T t)
    {
        InvokeAsync(FindObjectOfType<MonoBehaviour>(),t);
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