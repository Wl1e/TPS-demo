using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct RangedFloat: INetworkSerializable
{
    [SerializeField] float value;
    [SerializeField] float lo;
    [SerializeField] float hi;
    public float Value => value;

    public float Ratio() => (value - lo) / (hi - lo);
    public bool IsLow() => value == lo;
    public bool IsHigh() => value == hi;

    public void SetHigh(float high)
    {
        hi = high;
        value = Mathf.Clamp(value, lo, hi);
    }

    public float SetValue(float newValue)
    {
        float oldValue = value;
        value = Mathf.Clamp(newValue, lo, hi);
        return value - oldValue;
    }


    public float Add(float amount) => SetValue(value + amount);
    public float Subtract(float amount) => SetValue(value - amount);

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref lo);
        serializer.SerializeValue(ref hi);
        serializer.SerializeValue(ref value);
    }
}
