using UnityEngine;
using UnityEngine.Playables;

public class Set_IntAsset : PlayableAsset
{
    [SerializeField] private int constant;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<Set_IntPlayable>.Create(graph);
        var script = playable.GetBehaviour();
        script.val = constant;
        return playable;
    }
}