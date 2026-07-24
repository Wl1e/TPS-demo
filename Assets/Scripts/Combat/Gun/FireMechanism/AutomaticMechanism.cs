
using System;
using UnityEngine;

namespace TPSDemo
{
    public class AutomaticMechanism : MonoBehaviour, IFireMechanism
    {
        /// <summary>
        /// 射击间隔
        /// </summary>
        private float m_FireInternal;

        private float m_LastFiredTime = 0f;
        private bool m_IsFiring = false;

        public float FireInternal => m_FireInternal;
        public bool IsFiring => m_IsFiring;

        public event Action OnShouldFire;

        public void Initialize(IWeapon weapon)
        {
            m_FireInternal = weapon.Config.FireInternal;
        }

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
            if (!m_IsFiring || m_LastFiredTime + m_FireInternal > Time.time) {
                return;
            }
            m_LastFiredTime = Time.time;
            OnShouldFire?.Invoke();
        }
    }
}
