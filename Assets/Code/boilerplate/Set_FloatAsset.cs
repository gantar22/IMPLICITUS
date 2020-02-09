using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
/*

public class Set_VarAsset<T> : PlayableAsset
{
    [SerializeField] private T constant;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<Set_VarPlayable<T,Reference<T,UnityEvent<T>>>>.Create(graph);
        var script = playable.GetBehaviour();
        script.val = constant;
        return playable;
    }
}*/

public class Set_VarPlayable<T,TEvent,R> : PlayableBehaviour where TEvent : UnityEvent<T>, new() where R : Reference<T,TEvent>
{
    public T val;
    public R Reference;

    private float t = 0;

    public override void OnPlayableCreate(Playable playable)
    {
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Reference.val = val;   
    }
}

public class Set_FloatAsset : PlayableAsset
{
    [SerializeField] private float constant;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<Set_VarPlayable<float,FloatUnityEvent,Reference<float,FloatUnityEvent>>>.Create(graph);
        var script = playable.GetBehaviour();
        script.val = constant;
        return playable;
    }
}
