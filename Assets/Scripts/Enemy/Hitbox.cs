using UnityEngine;
using System;

namespace TPSDemo
{
    public class Hitbox : MonoBehaviour
    {
        [SerializeField] private Collider[] m_Hitbox;
        [SerializeField] private LayerMask m_CollisionLayer;

        public event Action<Damageable> OnCollision;

        public void Awake()
        {
            SetEnable(false);
        }

        public void SetEnable(bool enable)
        {
            foreach (var hitbox in m_Hitbox) {
                hitbox.enabled = enable;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other != null && other.TryGetComponent<Damageable>(out var damageable)) {
                OnCollision?.Invoke(damageable);
            }
        }
    }
}
