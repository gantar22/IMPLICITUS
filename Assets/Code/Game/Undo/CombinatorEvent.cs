using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CombinatorUnityEvent : UnityEvent<Combinator> { }

[CreateAssetMenu(menuName = "Framework/Events/Combinator")]
public class CombinatorEvent : EventObject<Combinator, CombinatorUnityEvent> { }
