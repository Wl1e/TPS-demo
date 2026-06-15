using System;
using UnityEngine;

namespace TPSDemo
{
    public class Health : MonoBehaviour
    {
        [SerializeField] RangedFloat m_HealthValue;
        public float CurrentHealth => m_HealthValue.Value;
        public float Ratio => m_HealthValue.Ratio();

        public Action<GameObject, float> OnTakeDamaged;
        public Action<int> OnDied;
        public Action<float> OnHealed;

        bool m_IsDied = false;

        public void TakeDamage(GameObject attacker, float damage)
        {
            if (m_IsDied) {
                return;
            }
            float trueDamage = -m_HealthValue.Subtract(damage);
            if (trueDamage > 0) {
                OnTakeDamaged?.Invoke(attacker, damage);
                if (gameObject.CompareTag("Player")) {
                    EventManager.Broadcast(new Event.HealthChangedEvent { value = trueDamage });
                }
            }
            HandleDeath(attacker);
        }

        void HandleDeath(GameObject attacker)
        {
            if (m_IsDied) {
                return;
            }
            if (m_HealthValue.IsLow()) {
                m_IsDied = true;
                int actorId = -1;
                if(attacker && attacker.TryGetComponent<Actor>(out var actor)) {
                    actorId = actor.Id;
                }
                OnDied?.Invoke(actorId);
            }
        }
    }
}
