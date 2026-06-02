using UnityEngine;

public class Shield: MonoBehaviour
{
    RangedFloat ShieldValue;
    public void SetMaxShield(float value) => ShieldValue.SetHigh(value);
}
