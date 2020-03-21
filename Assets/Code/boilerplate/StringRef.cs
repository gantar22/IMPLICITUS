
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StringUnityEvent : UnityEvent<string> {}
[CreateAssetMenu(menuName = "Framework/Reference/string")]
public class StringRef : Reference<string,StringUnityEvent> {}

