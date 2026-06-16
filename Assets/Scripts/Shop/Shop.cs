using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    [System.Serializable]
    public struct ShopEntry
    {
        public ItemData Good;
        public int Amount;
        public int Price;
        public float Discount;

        public int GoodId => Good.Id;
        public string GoodName => Good.Name;

        public int FinalPrice
        {
            get
            {
                int p = Mathf.RoundToInt(Price * Discount / 100f);
                return p > 0 ? p : 1;
            }
        }

        public ShopEntry(int itemId, int price, int amount = 1, float discount = 1f)
        {
            Good = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(itemId);
            Price = price;
            Amount = amount;
            Discount = discount;
        }
    }

    public class Shop : MonoBehaviour
    {
        public int ShopId;
        public string ShopName;
        public bool Restock;
        public float RestockTime;
        public bool RandomGoods;
        private int m_MoneyId;

        List<ShopEntry> m_Goods;
        public List<ShopEntry> Goods => m_Goods;
        public int GoodCount => m_Goods.Count;

        PlayerController m_Player;

        private void OnEnable()
        {
            EventManager.AddListener<Event.TryBuyEvent>(TryBuy);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Event.TryBuyEvent>(TryBuy);
        }

        public void Initialize(ShopConfig config)
        {
            ShopId = config.ShopId;
            ShopName = config.ShopName;
            m_MoneyId = config.MoneyId;
            Restock = config.Restock;
            RestockTime = config.RestockTime;
            RandomGoods = config.RandomGoods;
            // 修改会同步到SO，变相的存储?
            // m_Goods = config.Goods?.Count > 0 ? config.Goods : new List<ShopEntry>();
            m_Goods = new List<ShopEntry>(config.Goods);
        }

        public void AddGood(int itemId, int price, int amount = 1, float discount = 1f)
        {
            m_Goods.Add(new ShopEntry(itemId, price, amount, discount));
        }

        public void AddGood(ShopEntry entry)
        {
            m_Goods.Add(entry);
        }

        public bool RemoveGood(int slot)
        {
            if (slot < 0 || slot >= m_Goods.Count)
                return false;
            // 如果直接RemoveAt，会导致UI的slot和logic的slot不同步
            // m_Goods.RemoveAt(slot);
            m_Goods[slot] = default;
            return true;
        }

        public ShopEntry? GetGood(int slot)
        {
            if (slot < 0 || slot >= m_Goods.Count)
                return null;
            return m_Goods[slot];
        }


        public void Enter(PlayerController player)
        {
            m_Player = player;
            m_Player.SetInputActive(false, false);
            EventManager.Broadcast(new Event.ShopOpenEvent { ShopId = ShopId, ShopName = ShopName, Goods = Goods });
        }

        public void TryBuy(Event.TryBuyEvent evt)
        {
            int slot = evt.Slot;
            if (slot < 0 || slot >= m_Goods.Count) {
                Debug.LogError("Err Goods Slot: " + slot);
                return;
            }



            var good = m_Goods[slot];
            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(good.GoodId);
            if (itemData == null) {
                Debug.LogError($"ItemData not found for id: {good.GoodId}");
                return;
            }
            if (m_Player) {
                Buy(m_Player, slot);
            }
        }

        public bool Buy(PlayerController player, int slot)
        {
            var goodEntry = m_Goods[slot];
            int finalPrice = goodEntry.FinalPrice;
            var itemData = goodEntry.Good;

            bool isSuccess = false;
            string failInfo = "";
            if (player.Economy.CanAfford(m_MoneyId, finalPrice)) {
                player.Economy.SpendMoney(m_MoneyId, finalPrice);
                RemoveGood(slot);
                player.Inventory.AddItem(itemData.Id, goodEntry.Amount);
                isSuccess = true;
            } else {
                isSuccess = false;
                failInfo = $"购买{itemData.Name}失败，金币不足";
            }

            EventManager.Broadcast(
                new Event.ShopBuyEvent {
                    ShopId = ShopId,
                    Slot = slot,
                    Price = finalPrice,
                    IsSuccess = isSuccess,
                    FailInfo = failInfo
                }
            );

            if (isSuccess) {
                EventManager.Broadcast(
                    new Event.MessageLogEvent {
                        Message = $"Player{player.Id} 购买{itemData.Name}成功"
                    }
                );
            } else {
                EventManager.Broadcast(
                    new Event.MessageLogEvent {
                        Message = $"Player{player.Id} 购买{itemData.Name}失败，金币不足"
                    }
                );
            }

            return isSuccess;
        }

        public void Exit(PlayerController player)
        {
            player.SetInputActive(true, true);
            EventManager.Broadcast(new Event.ShopCloseEvent { ShopId = ShopId });
        }
    }
}
