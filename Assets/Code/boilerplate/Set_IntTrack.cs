using UnityEngine.Timeline;

[TrackBindingType(typeof(Reference<int,IntUnityEvent>))]
[TrackClipType(typeof(Set_IntAsset))]
public class Set_IntTrack : TrackAsset {}