using UnityEngine;
using UnityEngine.Events;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator, Lambda.Variable>>;

[System.Serializable]
public class TermUnityEvent : UnityEvent<Term> { }

[CreateAssetMenu(menuName = "Framework/Events/Term")]
public class TermEvent : EventObject<Term, TermUnityEvent> { }
