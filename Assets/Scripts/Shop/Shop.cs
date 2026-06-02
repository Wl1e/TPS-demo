using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ShopEntry
{
    public string ItemName;
    public int ItemId;
    public int Amount;
    public int Price;
    public float Discount;

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
        ItemName = ItemDataList.GetItemData(itemId).Name;
        ItemId = itemId;
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

    List<ShopEntry> m_Goods;
    public List<ShopEntry> Goods => m_Goods;
    public int GoodCount => m_Goods.Count;

    PlayerController m_Player;

    void Awake()
    {
        ShopManager.Instance.Register(this);
    }

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
        Restock = config.Restock;
        RestockTime = config.RestockTime;
        RandomGoods = config.RandomGoods;
        m_Goods = config.Goods?.Count > 0 ? config.Goods : new List<ShopEntry>();
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
        if (slot < 0 || slot >= m_Goods.Count) return false;
        m_Goods.RemoveAt(slot);
        return true;
    }

    public ShopEntry? GetGood(int slot)
    {
        if (slot < 0 || slot >= m_Goods.Count) return null;
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
        var itemData = ItemDataList.GetItemData(good.ItemId);
        if (itemData == null) {
            Debug.LogError($"ItemData not found for id: {good.ItemId}");
            return;
        }
        if (m_Player) {
            Buy(m_Player, slot);
        }
    }

    public bool Buy(PlayerController player, int slot)
    {
        var good = m_Goods[slot];
        int finalPrice = good.FinalPrice;
        var itemData = ItemDataList.GetItemData(good.ItemId);

        bool isSuccess = false;
        string failInfo = "";
        if (player.Economy.CanAfford(finalPrice)) {
            player.Economy.SpendMoney(finalPrice);
            RemoveGood(slot);
            player.Inventory.AddItem(itemData.Id, good.Amount);
            isSuccess = true;
        } else {
            Debug.Log($"Not enough money. Need: {finalPrice}, Have: {player.Economy.Money}");
            isSuccess = false;
            failInfo = $"¹ºÂò{itemData.Name}Ê§°Ü£¬½ð±Ò²»×ã";
        }

        EventManager.Broadcast(
            new Event.ShopBuyEvent{
                ShopId = ShopId,
                Slot = slot,
                Price = finalPrice,
                IsSuccess = isSuccess,
                FailInfo = failInfo
            }
        );
        return true;
    }

    public void Exit(PlayerController player)
    {
        player.SetInputActive(true, true);
        EventManager.Broadcast(new Event.ShopCloseEvent { ShopId = ShopId });
    }
}
