using UnityEngine;
using System;

namespace TPSDemo
{
    public class Hitbox : MonoBehaviour
    {
        [SerializeField] private Collider[] m_Hitbox;
        [SerializeField] private LayerMask m_CollisionLayer;

        private readonly System.Collections.Generic.List<GameObject> m_Targets = new();
        public System.Collections.Generic.List<GameObject> Targets => m_Targets;

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

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) {
                return;
            }
            if(other.TryGetComponent<Damageable>(out var damageable)) {
                OnCollision?.Invoke(damageable);
                if(!m_Targets.Contains(damageable.Owner)) {
                    m_Targets.Add(damageable.Owner);
                }
            } else {
                if (!m_Targets.Contains(other.gameObject)) {
                    m_Targets.Add(other.gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null) {
                return;
            }
            if (other.TryGetComponent<Damageable>(out var damageable)) {
                if (m_Targets.Contains(damageable.Owner)) {
                    m_Targets.Remove(damageable.Owner);
                }
            } else {
                if (m_Targets.Contains(other.gameObject)) {
                    m_Targets.Remove(other.gameObject);
                }
            }
        }
    }
}
