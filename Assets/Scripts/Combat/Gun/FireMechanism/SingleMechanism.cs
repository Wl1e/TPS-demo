using System;
using UnityEngine;

namespace TPSDemo
{
    public class SingleMechanism : MonoBehaviour, IFireMechanism
    {
        [Tooltip("射击间隔")]
        [SerializeField] float m_FireInternal;
        [Tooltip("是否为释放射击")]
        [SerializeField] bool m_ReleaseTrigger = false;

        bool m_Held = false;

        private float m_FireCD = 0f;

        public float FireInternal { get; }
        public bool IsFiring { get; }

        public event Action OnShouldFire;

        public void StartFire()
        {
            m_Held = true;
            if (!m_ReleaseTrigger && m_Held && m_FireCD <= 0f) {
                OnShouldFire?.Invoke();
                m_FireCD = m_FireInternal;
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

        private void Update()
        {
            if (m_FireCD > 0f) {
                m_FireCD -= Time.deltaTime;
            }
        }
    }
}
