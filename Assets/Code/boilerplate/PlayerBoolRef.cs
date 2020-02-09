
using UnityEngine;
using UnityEngine.Events;

public class BoolArrayEvent : UnityEvent<bool[]> {}

[CreateAssetMenu(menuName = "Framework/Reference/Player Bool")]
public class PlayerBoolRef : PlayerRef<bool,BoolArrayEvent> {}