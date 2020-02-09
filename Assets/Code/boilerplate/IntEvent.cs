using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntUnityEvent : UnityEvent<int> {}

[System.Serializable]
public class BoolUnityEvent : UnityEvent<bool> { }

[CreateAssetMenu(menuName = "Framework/Events/Int")]
public class IntEvent : EventObject<int,IntUnityEvent> {}
