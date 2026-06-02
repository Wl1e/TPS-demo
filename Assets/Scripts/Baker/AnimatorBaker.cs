// AnimatorBaker.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class HumanoidBaker
{
    public static BakedAnimData Bake(AnimationClip clip, float sampleRate)
    {
        if (clip == null || sampleRate <= 0f)
            return null;

        AnimationCurve rootTX = GetRootCurve(clip, "RootT.x");
        AnimationCurve rootTY = GetRootCurve(clip, "RootT.y");
        AnimationCurve rootTZ = GetRootCurve(clip, "RootT.z");
        AnimationCurve rootQX = GetRootCurve(clip, "RootQ.x");
        AnimationCurve rootQY = GetRootCurve(clip, "RootQ.y");
        AnimationCurve rootQZ = GetRootCurve(clip, "RootQ.z");
        AnimationCurve rootQW = GetRootCurve(clip, "RootQ.w");

        bool hasPosition = rootTX != null || rootTZ != null;
        bool hasRotation = rootQX != null || rootQY != null || rootQZ != null || rootQW != null;

        float dt = 1f / sampleRate;
        int frameCount = Mathf.Max(1, Mathf.CeilToInt(clip.length * sampleRate) + 1);

        var velocityXKeys = new List<Keyframe>();
        var velocityZKeys = new List<Keyframe>();
        var rotationKeys = new List<Keyframe>();

        float prevX = rootTX?.Evaluate(0f) ?? 0f;
        float prevY = rootTY?.Evaluate(0f) ?? 0f;
        float prevZ = rootTZ?.Evaluate(0f) ?? 0f;
        Quaternion prevRot = hasRotation
            ? NormalizeQuaternion(new Quaternion(
                rootQX?.Evaluate(0f) ?? 0f,
                rootQY?.Evaluate(0f) ?? 0f,
                rootQZ?.Evaluate(0f) ?? 0f,
                rootQW?.Evaluate(0f) ?? 1f))
            : Quaternion.identity;

        for (int i = 1; i < frameCount; i++) {
            float t = Mathf.Min(i * dt, clip.length);

            float x = rootTX?.Evaluate(t) ?? 0f;
            float y = rootTY?.Evaluate(t) ?? 0f;
            float z = rootTZ?.Evaluate(t) ?? 0f;
            Quaternion rot = hasRotation
                ? NormalizeQuaternion(new Quaternion(
                    rootQX?.Evaluate(t) ?? 0f,
                    rootQY?.Evaluate(t) ?? 0f,
                    rootQZ?.Evaluate(t) ?? 0f,
                    rootQW?.Evaluate(t) ?? 1f))
                : Quaternion.identity;

            Vector3 deltaPos = new Vector3(x - prevX, y - prevY, z - prevZ);
            Vector3 velocity = Quaternion.Inverse(rot) * (deltaPos / dt);
            velocity.y = 0f;

            Quaternion rotateDelta = Quaternion.Inverse(prevRot) * rot;
            rotateDelta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f)
                angle -= 360f;
            float yRotationSpeed = 0f;
            if (Mathf.Abs(angle) > 0.001f && axis.sqrMagnitude > 0.001f) {
                float yComponent = Mathf.Abs(Vector3.Dot(axis.normalized, Vector3.up));
                yRotationSpeed = angle * yComponent / dt;
            }

            velocityXKeys.Add(new Keyframe(t, velocity.x));
            velocityZKeys.Add(new Keyframe(t, velocity.z));
            rotationKeys.Add(new Keyframe(t, yRotationSpeed));

            prevX = x;
            prevY = y;
            prevZ = z;
            prevRot = rot;
        }

        var data = ScriptableObject.CreateInstance<BakedAnimData>();
        data.ClipName = clip.name;
        data.ClipLength = clip.length;
        data.SampleRate = sampleRate;

        data.VelocityXCurve = velocityXKeys.Count > 0
            ? new AnimationCurve(velocityXKeys.ToArray())
            : new AnimationCurve();
        data.VelocityZCurve = velocityZKeys.Count > 0
            ? new AnimationCurve(velocityZKeys.ToArray())
            : new AnimationCurve();
        data.RotationCurve = rotationKeys.Count > 0
            ? new AnimationCurve(rotationKeys.ToArray())
            : new AnimationCurve();

        return data;
    }

    static AnimationCurve GetRootCurve(AnimationClip clip, string propertyName)
    {
        var bindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in bindings) {
            if (binding.path == "" && binding.type == typeof(Animator) && binding.propertyName == propertyName) {
                return AnimationUtility.GetEditorCurve(clip, binding);
            }
        }
        return null;
    }

    static Quaternion NormalizeQuaternion(Quaternion q)
    {
        float mag = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        if (mag > 0.0001f)
            return new Quaternion(q.x / mag, q.y / mag, q.z / mag, q.w / mag);
        return Quaternion.identity;
    }
}