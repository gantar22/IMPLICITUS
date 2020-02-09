using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Set_FloatPlayable : PlayableBehaviour
{
    public float val;
    public AnimationCurve curve;
    public Reference<float,FloatUnityEvent> Reference;

    private float t = 0;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (curve != null)
        {
            t += info.deltaTime;
            Reference.val = curve.Evaluate((float) playable.GetTime() / (float) playable.GetDuration());
        }
        else
        {
            Reference.val = val;    
        }
        
    }
}
