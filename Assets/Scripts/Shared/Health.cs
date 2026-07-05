using System;
using UnityEngine;

namespace TPSDemo
{
    public class Health : MonoBehaviour
    {
        [SerializeField] RangedFloat m_HealthValue;
        public float CurrentHealth => m_HealthValue.Value;
        public float Ratio => m_HealthValue.Ratio();

        public event Action<DamageInfo> OnTakeDamaged;
        public Action<int> OnDied;
        public Action<float> OnHealed;

        bool m_IsDied = false;

        public float TakeDamage(DamageInfo info)
        {
            if (m_IsDied) {
                return 0;
            }
            float trueDamage = -m_HealthValue.Subtract(info.Damage);
            if (trueDamage > 0) {
                OnTakeDamaged?.Invoke(info);
                if (gameObject.CompareTag("Player")) {
                    EventManager.Broadcast(new Event.HealthChangedEvent { value = trueDamage });
                }
            }
            HandleDeath(info.Attacker);
            return trueDamage;
        }

        public void Heal(float value)
        {
            var trueHealValue = m_HealthValue.Add(value);
            OnHealed?.Invoke(trueHealValue);
            if (gameObject.CompareTag("Player")) {
                EventManager.Broadcast(new Event.HealthChangedEvent { value = trueHealValue });
            }
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
