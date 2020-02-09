using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Framework/Events/Bool")]
public class BoolEvent : EventObject<bool, BoolUnityEvent>
{
    public void AddListener(Action trueCase, Action falseCase)
    {
        AddListener(b => { if (b) trueCase(); else falseCase(); });
    }

    public void AddListener<T>(Func<T> trueCase, Func<T> falseCase)
    {
        AddListener(trueCase, falseCase);
    }

}