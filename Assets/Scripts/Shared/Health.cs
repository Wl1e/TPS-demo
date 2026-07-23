using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class Health : NetworkBehaviour
    {
        [SerializeField] private NetworkVariable<RangedFloat> m_HealthValue;
        public float CurrentHealth => m_HealthValue.Value.Value;
        public float Ratio => m_HealthValue.Value.Ratio();

        public event Action<DamageInfo> OnTakeDamaged;
        public Action<int> OnDied;
        public Action<float> OnHealed;

        public Action<float> OnHealthChanged;

        bool IsDied => m_HealthValue.Value.IsLow();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_HealthValue.OnValueChanged += HealthValueChanged;
        }

        private void HealthValueChanged(RangedFloat previousValue, RangedFloat newValue)
        {
            OnHealthChanged?.Invoke(newValue.Value - previousValue.Value);
        }

        public float TakeDamage(DamageInfo info)
        {
            if (IsDied) {
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

        public void Revive() => m_HealthValue.Value.FullHealth();

        void HandleDeath(GameObject attacker)
        {
            if (IsDied) {
                int actorId = -1;
                if(attacker && attacker.TryGetComponent<Actor>(out var actor)) {
                    actorId = actor.Id;
                }
                OnDied?.Invoke(actorId);
            }
        }
    }
}
