using System;
using UnityEngine;

namespace TPSDemo
{
    public class SingleMechanism : MonoBehaviour, IFireMechanism
    {
        [SerializeField]
        float m_FireInternal;
        [SerializeField]
        bool m_ReleaseTrigger = false;

        bool m_Held = false;

        public float FireInternal { get; }
        public bool IsFiring { get; }

        public event Action OnShouldFire;

        public void StartFire()
        {
            m_Held = true;
            if (!m_ReleaseTrigger && m_Held) {
                OnShouldFire?.Invoke();
            }
        }
        public void StopFire()
        {
            if (m_Held && m_ReleaseTrigger) {
                OnShouldFire?.Invoke();
            }
            m_Held = false;
        }

        public void UpdateFire(float deltaTime)
        {
        }
    }
}
