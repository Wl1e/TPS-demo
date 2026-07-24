
using System;
using UnityEngine;

namespace TPSDemo
{
    public class Burst : MonoBehaviour, IFireMechanism
    {
        /// <summary>
        /// 一次发射多少发
        /// </summary>
        [SerializeField]
        private int m_ShotPerBurst;
        /// <summary>
        /// 射击间隔
        /// </summary>
        private float m_ShotInternal;
        private int m_CurrentShot;
        private float m_LastShotTime = 0f;

        private bool m_IsFiring = false;
        public float FireInternal { get; }
        public bool IsFiring => m_IsFiring;

        public event Action OnShouldFire;

        public void Initialize(IWeapon weapon)
        {
            m_ShotInternal = weapon.Config.FireInternal;
        }

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
            if (!m_IsFiring) {
                return;
            }
            if (m_CurrentShot >= m_ShotPerBurst) {
                return;
            }
            if (m_LastShotTime + m_ShotInternal > Time.time) {
                return;
            }
            m_LastShotTime = Time.time;
            OnShouldFire?.Invoke();
            m_CurrentShot++;
        }
    }
}
