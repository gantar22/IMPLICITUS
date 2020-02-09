using UnityEngine;
using UnityEngine.Events;

public class FloatUnityEvent : UnityEvent<float> {}

[CreateAssetMenu(menuName = "Framework/Reference/Float")]

public class FloatRef : Reference<float,FloatUnityEvent> { }
