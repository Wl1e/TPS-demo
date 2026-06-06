
using System;
using System.Collections.Generic;

namespace TPSDemo
{
    public class PlayerDataProxy
    {
        static PlayerDataProxy m_Instance;
        public static PlayerDataProxy Instance => m_Instance ?? (m_Instance = new PlayerDataProxy());
        PlayerController m_Player;

        public void RegisterPlayer(PlayerController player)
        {
            m_Player = player;
        }
        public void UnregisterPlayer()
        {
            m_Player = null;
        }
        public bool HasPlayer()
        {
            return m_Player != null;
        }

        public float GetHealthRatio()
        {
            return m_Player.Health.Ratio;
        }

        public IWeapon GetCurrentFirearm()
        {
            Console.WriteLine(Instance.m_Player.ToString());
            return m_Player.WeaponManager.CurrentFirearm;
        }
        public int GetCurrentFirearmIdx()
        {
            return m_Player.WeaponManager.CurrentFirearmIndex;
        }

        public List<(int, int)> GetInventoryData()
        {
            return m_Player.Inventory.GetAllItem();
        }
        public int GetInventoryAmountByType(ItemType type)
        {
            var currentFirearm = m_Player.WeaponManager.CurrentFirearm;
            if (currentFirearm == null) {
                return 0;
            }
            return m_Player.Inventory.GetAmount(currentFirearm.AmmoId);
        }

        // ������Play
        public List<ShopEntry> GetShopGoods(int shopId)
        {
            return ShopManager.Instance.GetGoods(shopId);
        }

        // 结构同UpdateLoadoutUIEvent
        public List<UI.WeaponUIData> GetLoadoutData()
        {
            return m_Player.Loadout.GetUIData();
        }
    }
}
