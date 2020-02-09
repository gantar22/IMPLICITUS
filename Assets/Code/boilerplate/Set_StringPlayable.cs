using UnityEngine;
using UnityEngine.Playables;

public class Set_StringPlayable : PlayableBehaviour
{
    [TextArea]
    public string val;
    public StringRef Reference;

    private float t = 0;

    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        StringRef ref_in = playerData as StringRef;
        Reference = ref_in;
        Reference.val = val;   
    }
}