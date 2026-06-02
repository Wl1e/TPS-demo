
using System;
using UnityEngine;

public class Burst: MonoBehaviour, IFireMechanism
{
    [SerializeField]
    int m_ShotPerBurst;
    [SerializeField]
    float m_ShotInternal;
    int m_CurrentShot;
    float m_LastShotTime = 0f;

    bool m_IsFiring = false;
    public float FireInternal { get; }
    public bool IsFiring => m_IsFiring;

    public event Action OnShouldFire;

    public void StartFire()
    {
        m_IsFiring = true;
        m_CurrentShot = 0;
    }
    public void StopFire()
    {
        m_IsFiring = false;
    }
    public void UpdateFire(float deltaTime)
    {
        if(!m_IsFiring) {
            return;
        }
        if(m_CurrentShot >= m_ShotPerBurst) {
            return;
        }
        if(m_LastShotTime + m_ShotInternal > Time.time) {
            return;
        }
        m_LastShotTime = Time.time;
        OnShouldFire?.Invoke();
        m_CurrentShot++;
    }
}