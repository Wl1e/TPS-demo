using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    [System.Serializable]
    public struct ShopEntry: INetworkSerializable
    {
        public string GoodName;
        public int GoodId;
        public int Amount;
        public int Price;
        public float Discount;
        public bool Soldout;

        public readonly int FinalPrice
        {
            get => Mathf.Min(1, Mathf.RoundToInt(Price * Discount / 100f));
        }

        public ShopEntry(ItemData item, int price, int amount = 1, float discount = 100f)
        {
            GoodId = item.Id;
            GoodName = item.Name;
            Price = price;
            Amount = amount;
            Discount = discount;
            Soldout = false;
        }

        void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            serializer.SerializeValue(ref GoodName);
            serializer.SerializeValue(ref GoodId);
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref Price);
            serializer.SerializeValue(ref Discount);
            serializer.SerializeValue(ref Soldout);

        }
    }

    public class Shop
    {
        public int ShopId;
        public string ShopName => GetConfig().ShopName;
        public bool Restock => GetConfig().Restock;
        public float RestockTime => GetConfig().RestockTime;
        public bool RandomGoods => GetConfig().RandomGoods;
        public int MoneyId => GetConfig().MoneyId;

        List<ShopEntry> m_Goods = new();
        public List<ShopEntry> Goods => m_Goods;
        public ShopEntry[] GoodArr => m_Goods.ToArray();
        public int GoodCount => m_Goods.Count;

        PlayerController m_Player;

        public Shop(ShopConfig config)
        {
            ShopId = config.ShopId;
            // 修改会同步到SO，变相的存储?
            // m_Goods = config.Goods?.Count > 0 ? config.Goods : new List<ShopEntry>();
            foreach(var goodConfig in config.Goods) {
                AddGood(goodConfig.Good, goodConfig.Price, goodConfig.Amount, goodConfig.Discount);
            }
        }

        private ShopConfig GetConfig() => ResourceManager.Instance.GetResource<ShopList>("Shop").GetConfig(ShopId);

        public void SetGoods(List<ShopEntry> newGoods) => m_Goods = newGoods;
        public void SetGoods(ShopEntry[] newGoods) => m_Goods = new(newGoods);

        public void AddGood(ItemData item, int price, int amount = 1, float discount = 100f) => m_Goods.Add(new ShopEntry(item, price, amount, discount));

        public void AddGood(ShopEntry entry) => m_Goods.Add(entry);

        public void GoodSoldout(int slot)
        {
            if (slot < 0 || slot >= m_Goods.Count) {
                return;
            }
            // 如果直接RemoveAt，会导致UI的slot和logic的slot不同步
            // m_Goods.RemoveAt(slot);
            ShopEntry good = m_Goods[slot];
            good.Soldout = true;
            m_Goods[slot] = good;
            Debug.Log($"Set {slot} Soldout");
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
            //EventManager.AddListener<Event.TryBuyEvent>(TryBuy);
            EventManager.Broadcast(new Event.ShopOpenEvent { ShopId = ShopId, ShopName = ShopName });
        }

        public void Exit(PlayerController player)
        {
            //EventManager.RemoveListener<Event.TryBuyEvent>(TryBuy);
            EventManager.Broadcast(new Event.ShopCloseEvent { ShopId = ShopId });
        }

        #region Server
        /// <summary>
        /// Only call by server
        /// </summary>
        public Event.ShopBuyEvent Buy(PlayerController player, int slot)
        {
            var evt = new Event.ShopBuyEvent {
                ShopId = ShopId,
                Slot = slot,
                Price = 0,
                IsSuccess = false,
                FailInfo = ""
            };

            if (slot < 0 || slot >= m_Goods.Count) {
                evt.FailInfo = "错误商店槽位 " + slot;
                return evt;
            }

            var good = m_Goods[slot];
            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(good.GoodId);
            if (itemData == null) {
                evt.FailInfo = $"不存在道具: {good.GoodId}";
                return evt;
            }

            var goodEntry = m_Goods[slot];
            int finalPrice = goodEntry.FinalPrice;

            if (player.Economy.CanAfford(MoneyId, finalPrice)) {
                player.Economy.SpendMoney(MoneyId, finalPrice);
                GoodSoldout(slot);
                player.Inventory.AddItemClientRpc(player.Id, itemData.Id, goodEntry.Amount);
                evt.IsSuccess = true;
            } else {
                evt.IsSuccess = false;
                evt.FailInfo = $"购买{itemData.Name}失败，金币不足";
            }

            evt.Price = finalPrice;

            return evt;
        }

        #endregion

    }
}
