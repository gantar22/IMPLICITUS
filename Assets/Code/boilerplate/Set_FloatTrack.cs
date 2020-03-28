using System.Collections;
using System.Collections.Generic;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Reference<float,FloatUnityEvent>))]
[TrackClipType(typeof(Set_FloatAsset))]
public class Set_FloatTrack : TrackAsset
{}