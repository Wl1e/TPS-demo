using UnityEngine;

public class BakedAnimData : ScriptableObject
{
    public string ClipName;
    public float ClipLength;
    public float SampleRate;
    public AnimationCurve VelocityXCurve;
    public AnimationCurve VelocityZCurve;
    public AnimationCurve RotationCurve;
}
