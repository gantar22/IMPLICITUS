using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerRef<T,EventType> : Reference<T[],EventType> where EventType : UnityEvent<T[]>, new()
{
    public T this[int index] 
    { 
        get => val[index];
        set => val[index] = value; 
    }
}
