using UnityEngine;
using UnityEngine.Playables;

public class Set_StringAsset : PlayableAsset
{
    [TextArea]
    [SerializeField] private string constant;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<Set_StringPlayable>.Create(graph);
        var script = playable.GetBehaviour();
        script.val = constant;
        return playable;
    }
}