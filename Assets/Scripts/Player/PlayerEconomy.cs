using System;
using UnityEngine;

public class PlayerEconomy : MonoBehaviour
{
    [SerializeField] int m_Money;
    public int Money => m_Money;

    public Action<int> OnMoneyChanged;

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        m_Money += amount;
        OnMoneyChanged?.Invoke(m_Money);
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return false;
        if (m_Money < amount) return false;
        m_Money -= amount;
        OnMoneyChanged?.Invoke(m_Money);
        return true;
    }

    public bool CanAfford(int amount)
    {
        return amount > 0 && m_Money >= amount;
    }
}
