using UnityEngine;
using UnityEngine.Events;

public class Vector3UnityEvent : UnityEvent<Vector3> {}

[CreateAssetMenu(menuName = "Framework/Reference/Vector3")]
public class Vector3Ref : Reference<Vector3,Vector3UnityEvent> { }
