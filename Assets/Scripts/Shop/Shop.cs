using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    [System.Serializable]
    public class ShopEntry: INetworkSerializable
    {
        public string GoodName = "";
        public int GoodId = 0;
        public int Amount = 0;
        public int Price = 0;
        public float Discount = 0f;
        public bool Soldout = true;
        public bool Restocking = false;
        public float RestockTime = 0f;

        public int FinalPrice
        {
            get => Mathf.Min(1, Mathf.RoundToInt(Price * Discount / 100f));
        }

        public ShopEntry() { }

        public ShopEntry(
            ItemData item,
            int price,
            int amount = 1,
            float discount = 100f
        )
        {
            GoodId = item.Id;
            GoodName = item.Name;
            Price = price;
            Amount = amount;
            Discount = discount;
            Soldout = false;
            Restocking = false;
            RestockTime = 0f;
        }

        void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            serializer.SerializeValue(ref GoodName);
            serializer.SerializeValue(ref GoodId);
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref Price);
            serializer.SerializeValue(ref Discount);
            serializer.SerializeValue(ref Soldout);
            serializer.SerializeValue(ref Restocking);
            serializer.SerializeValue(ref RestockTime);
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

        public void SetGoods(List<ShopEntry> newGoods)
        {
            m_Goods = newGoods;
            for (int i = 0; i < m_Goods.Count; i++) {
                UpdateGoodState(i);
            }
        }
        public void SetGoods(ShopEntry[] newGoods) => m_Goods = new(newGoods);
        /// <summary>
        /// 由Server同步给各个Client，Client更新数据
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="good"></param>
        public void SetGood(int slot, ShopEntry good)
        {
            //Debug.Log($"slot {slot} become {good.Restocking} {good.RestockTime}");
            m_Goods[slot] = good;
            UpdateGoodState(slot);
        }

        public void AddGood(ItemData item, int price, int amount = 1, float discount = 100f) => m_Goods.Add(new ShopEntry(item, price, amount, discount));

        public void AddGood(ShopEntry entry) => m_Goods.Add(entry);

        public void UpdateGoodState(int slot)
        {
            if (slot < 0 || slot >= m_Goods.Count) {
                return;
            }
            // 如果直接RemoveAt，会导致UI的slot和logic的slot不同步
            // m_Goods.RemoveAt(slot);
            ShopEntry good = m_Goods[slot];
            if(good.Restocking) {
                ShopManager.Instance.StartRestockCoroutine(this, slot);
                Debug.Log($"Good {slot} Restock");
            } else if(good.Soldout) {
                Debug.Log($"Good {slot} Soldout");
            }
            EventManager.Broadcast(new Event.ShopUpdateEvent {
                ShopId = ShopId,
                Slot = slot
            });
        }

        /// <summary>
        /// 补货商品
        /// </summary>
        /// <param name="slot"></param>
        public void RestockGood(int slot)
        {
            Debug.Log("slot " + slot + "finish Restock");
            m_Goods[slot].Restocking = false;
            m_Goods[slot].RestockTime = 0f;
            EventManager.Broadcast(new Event.ShopUpdateEvent {
                ShopId = ShopId,
                Slot = slot
            });
        }

        /// <summary>
        /// 获取单格商品
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public ShopEntry GetGood(int slot)
        {
            if (slot < 0 || slot >= m_Goods.Count) {
                return null;
            }
            return m_Goods[slot];
        }

        /// <summary>
        /// 商店进入
        /// </summary>
        /// <param name="player"></param>
        public void Enter(PlayerController player)
        {
            m_Player = player;
            //EventManager.AddListener<Event.TryBuyEvent>(TryBuy);
            EventManager.Broadcast(new Event.OpenShopUIEvent { ShopId = ShopId, ShopName = ShopName });
        }

        /// <summary>
        /// 商店退出
        /// </summary>
        public void Exit(PlayerController player)
        {
            //EventManager.RemoveListener<Event.TryBuyEvent>(TryBuy);
            EventManager.Broadcast(new Event.CloseShopUIEvent { ShopId = ShopId });
        }

        #region Server
        /// <summary>
        /// 商店购买的逻辑
        /// Only call by server
        /// </summary>
        public Event.ShopBuyEvent Buy(PlayerController player, int slot)
        {
            var evt = new Event.ShopBuyEvent {
                ShopId = ShopId,
                Slot = slot,
                Price = 0,
                IsSuccess = false,
                Info = $"商店{ShopId}: "
            };

            var good = GetGood(slot);
            if(good == null) {
                evt.Info += "错误商店槽位 " + slot;
                return evt;
            }

            if(good.Soldout || good.Restocking) {
                evt.Info += "商品已售空或补货中";
                return evt;
            }

            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(good.GoodId);
            if (itemData == null) {
                evt.Info += $"不存在道具: {good.GoodId}";
                return evt;
            }

            int finalPrice = good.FinalPrice;

            if (player.Economy.CanAfford(MoneyId, finalPrice)) {
                player.Economy.SpendMoney(MoneyId, finalPrice);
                player.Inventory.AddItemClientRpc(player.Id, itemData.Id, good.Amount);
                evt.IsSuccess = true;

                good.Soldout = !Restock;
                good.Restocking = Restock;
                good.RestockTime = RestockTime;
                evt.Info += $"购买{itemData.Name}成功";
            } else {
                evt.IsSuccess = false;
                evt.Info += $"购买{itemData.Name}失败，金币不足";
            }

            evt.Price = finalPrice;
            return evt;
        }

        #endregion

    }
}
