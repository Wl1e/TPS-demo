using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class PlayerEconomy: NetworkBehaviour
    {
        public List<Vector2Int> DefaultEconomy;
        private Dictionary<int, int> m_Money = new();

        // OnEconomyChanged无法得知发生变化的金币是哪一个，让需要的自己拿
        public event Action OnMoneyChanged;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) {
                foreach (var economy in DefaultEconomy) {
                    m_Money[economy.x] = economy.y;
                }
                SyncEconomyClientRpc(DefaultEconomy.ToArray());
            }
        }

        [ClientRpc]
        private void OnEconomyChangedClientRpc(int moneyId, int newValue)
        {
            if(!IsOwner) {
                return;
            }
            m_Money[moneyId] = newValue;
            OnMoneyChanged?.Invoke();
            EventManager.Broadcast(new Event.PlayerEconomyChangedEvent { MoneyId = moneyId, Amount = m_Money[moneyId] });
        }

        [ClientRpc]
        private void SyncEconomyClientRpc(Vector2Int[] money)
        {
            if(!IsOwner) {
                return;
            }
            foreach (var economy in money) {
                m_Money[economy.x] = economy.y;
            }
        }

        public int GetMoney(int moneyId)
        {
            return m_Money.GetValueOrDefault(moneyId, -1);
        }

        #region Server
        public void AddMoney(int moneyId, int amount)
        {
            if(!IsServer) {
                return;
            }
            if (amount <= 0) {
                return;
            }
            if (!m_Money.ContainsKey(moneyId)) {
                m_Money.Add(moneyId, 0);
            }
            m_Money[moneyId] += amount;
            OnEconomyChangedClientRpc(moneyId, m_Money[moneyId]);
        }

        public bool SpendMoney(int moneyId, int amount)
        {
            if (!IsServer) {
                return false;
            }
            if (amount <= 0) {
                return false;
            }
            if (!m_Money.ContainsKey(moneyId)) {
                return false;
            }
            if (m_Money[moneyId] < amount) {
                return false;
            }
            m_Money[moneyId] -= amount;
            OnEconomyChangedClientRpc(moneyId, m_Money[moneyId]);
            return true;
        }

        public bool CanAfford(int moneyId, int amount)
        {
            return amount > 0 && m_Money.GetValueOrDefault(moneyId, -1) >= amount;
        }

        #endregion
    }
}
