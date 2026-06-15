using System;
using System.Collections.Generic;

namespace TPSDemo
{
    public class PlayerEconomy
    {
        private Dictionary<int, int> m_Money = new();

        public Action<int, int> OnMoneyChanged;
        public int GetMoney(int moneyId) => m_Money.GetValueOrDefault(moneyId, -1);

        public void AddMoney(int moneyId, int amount)
        {
            if (amount <= 0) {
                return;
            }
            if (!m_Money.ContainsKey(moneyId)) {
                m_Money.Add(moneyId, 0);
            }
            m_Money[moneyId] += amount;
            OnMoneyChanged?.Invoke(moneyId, m_Money[moneyId]);
            EventManager.Broadcast(new Event.PlayerEconomyChangedEvent { MoneyId = moneyId, Amount = m_Money[moneyId] });
        }

        public bool SpendMoney(int moneyId, int amount)
        {
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
            OnMoneyChanged?.Invoke(moneyId, m_Money[moneyId]);
            EventManager.Broadcast(new Event.PlayerEconomyChangedEvent { MoneyId = moneyId, Amount = m_Money[moneyId] });
            return true;
        }

        public bool CanAfford(int moneyId, int amount)
        {
            return amount > 0 && m_Money.GetValueOrDefault(moneyId, -1) >= amount;
        }
    }
}
