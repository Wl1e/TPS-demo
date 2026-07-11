using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class Health : NetworkBehaviour
    {
        [SerializeField] NetworkVariable<RangedFloat> m_HealthValue;
        public float CurrentHealth => m_HealthValue.Value.Value;
        public float Ratio => m_HealthValue.Value.Ratio();

        public event Action<DamageInfo> OnTakeDamaged;
        public Action<int> OnDied;
        public Action<float> OnHealed;

        bool m_IsDied = false;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_HealthValue.OnValueChanged += HealthValueChanged;
        }

        private void HealthValueChanged(RangedFloat previousValue, RangedFloat newValue)
        {
            var diff = newValue.Value - previousValue.Value;
            if(newValue.Value <= 0f) {
            }
            if (diff > 0) {
            } else if (diff < 0) {
            }
        }

        public float TakeDamage(DamageInfo info)
        {
            if (m_IsDied) {
                return 0;
            }
            float trueDamage = -m_HealthValue.Value.Subtract(info.Damage);
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
            var trueHealValue = m_HealthValue.Value.Add(value);
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
            if (m_HealthValue.Value.IsLow()) {
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
