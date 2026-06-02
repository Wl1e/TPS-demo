
using System;
using UnityEngine;

public class AutomaticMechanism: MonoBehaviour, IFireMechanism
{
    [SerializeField]
    float m_FireInternal;

    float m_LastFiredTime = 0f;
    bool m_IsFiring = false;

    public float FireInternal => m_FireInternal;
    public bool IsFiring => m_IsFiring;

    public event Action OnShouldFire;

    public void StartFire()
    {
        m_IsFiring = true;
    }
    public void StopFire()
    {
        m_IsFiring = false;
    }

    public void UpdateFire(float deltaTime)
    {
        if(!m_IsFiring || m_LastFiredTime + m_FireInternal > Time.time) {
            return;
        }
        m_LastFiredTime = Time.time;
        OnShouldFire?.Invoke();
    }
}