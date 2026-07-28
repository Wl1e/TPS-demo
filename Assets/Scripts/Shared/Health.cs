using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class Health : NetworkBehaviour
    {
        // 使用SetDirty不是好方法，最好是不使用RangedFloat，而是NetworkVariable同步当前血量(float)
        // 但是RangedFloat我写都写了，不用不是可惜了
        [SerializeField] private NetworkVariable<RangedFloat> m_HealthValue;
        public float CurrentHealth => m_HealthValue.Value.Value;
        public float Ratio => m_HealthValue.Value.Ratio();

        public event Action<DamageInfo> OnTakeDamaged;
        public event Action<int> OnDied;
        public event Action<float> OnHealed;

        public event Action<float> OnHealthChanged;

        public bool IsDied => m_HealthValue.Value.IsLow();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_HealthValue.OnValueChanged += (pre, cur) => OnHealthChanged?.Invoke(cur.Value - pre.Value);
        }

        public float TakeDamage(DamageInfo info)
        {
            if (IsDied) {
                return 0;
            }
            float trueDamage = -m_HealthValue.Value.Subtract(info.Damage);
            if (info.Attacker && info.Attacker.TryGetComponent<NetworkObject>(out var no)) {
                TakeDamageRpc(no.NetworkObjectId, trueDamage, info.Point);
            } else {
                TakeDamageRpc(ulong.MaxValue, trueDamage, info.Point);
            }
            HandleDeath(info.Attacker);
            OnHealthChanged?.Invoke(trueDamage);
            m_HealthValue.SetDirty(true);
            return trueDamage;
        }

        public void Heal(float value)
        {
            var trueHealValue = m_HealthValue.Value.Add(value);
            OnHealthChanged?.Invoke(trueHealValue);
            HealRpc(trueHealValue);
            m_HealthValue.SetDirty(true);
        }

        [ServerRpc]
        public void ReviveServerRpc()
        {
            m_HealthValue.Value.FullHealth();
            m_HealthValue.SetDirty(true);
        }

        void HandleDeath(GameObject attacker)
        {
            if (IsDied) {
                int actorId = -1;
                if(attacker && attacker.TryGetComponent<Actor>(out var actor)) {
                    actorId = actor.Id;
                }
                DeathRpc(actorId);
            }
        }

        #region ToClient

        // 由于ActorId只在Server有效，所以使用NetworkObjectId传递
        [Rpc(SendTo.Owner)]
        private void TakeDamageRpc(ulong noId, float value, Vector3 point)
        {
            if(!IsOwner) {
                return;
            }
            GameNetworkManager.Instance.SpawnManager.SpawnedObjects.TryGetValue(noId, out var no);
            GameObject attacker = null;
            if (no) {
                attacker = no.GetComponentInChildren<Actor>().gameObject;
            }
            OnTakeDamaged?.Invoke(new DamageInfo {
                Attacker = attacker,
                Damage = value,
                Point = point
            });
        }

        [Rpc(SendTo.Owner)]
        private void HealRpc(float value)
        {
            if (!IsOwner) {
                return;
            }
            OnHealed?.Invoke(value);
        }

        [Rpc(SendTo.Owner)]
        private void DeathRpc(int actorId)
        {
            if (!IsOwner) {
                return;
            }
            OnDied?.Invoke(actorId);
        }

        #endregion
    }
}
