using System.Collections;
using System.Collections.Generic;
using TypeUtil;
using UnityEngine;
using UnityEngine.Events;

public class OnDestroyTrigger : MonoBehaviour
{
    public UnityEvent e = new UnityEvent();

    private void OnDestroy()
    {
        e.Invoke();
    }
}
