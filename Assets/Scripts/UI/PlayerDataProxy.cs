
using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace TPSDemo
{
    public class PlayerDataProxy
    {
        static PlayerDataProxy m_Instance;
        public static PlayerDataProxy Instance => m_Instance ?? (m_Instance = new PlayerDataProxy());
        PlayerController m_Player;

        public void RegisterPlayer(PlayerController player) => m_Player = player;
        public void UnregisterPlayer() => m_Player = null;
        public bool HasPlayer() => m_Player != null;
        public PlayerController GetPlayer() => m_Player;

        // health
        public float GetHealthRatio()
        {
            return m_Player.Health.Ratio;
        }

        // combat
        public IWeapon GetCurrentFirearm()
        {
            Console.WriteLine(Instance.m_Player.ToString());
            return m_Player.WeaponManager.CurrentFirearm;
        }
        public int GetCurrentFirearmIdx()
        {
            return m_Player.WeaponManager.CurrentFirearmIndex;
        }
        // 结构同UpdateLoadoutUIEvent
        public List<UI.WeaponUIData> GetLoadoutData()
        {
            return m_Player.Loadout.GetUIData();
        }

        // inventory
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

        // quest
        public NetworkList<QuestProcess> GetQuestProcesses() => m_Player.QuestController.QuestProcesses;

        //Shop
        public List<ShopEntry> GetShopGoods(int shopId)
        {
            var shop = ShopManager.Instance.GetCurrentShop();
            return shop != null ? shop.Goods : new List<ShopEntry>();
        }
        public int GetMoney(int shopId)
        {
            int moneyId = ResourceManager.Instance.GetResource<ShopList>("Shop").GetConfig(shopId).MoneyId;
            return m_Player.Economy.GetMoney(moneyId);
        }
    }
}
