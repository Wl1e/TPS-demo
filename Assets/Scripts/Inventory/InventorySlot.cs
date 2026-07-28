
using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public struct InventorySlotSync: INetworkSerializeByMemcpy, IEquatable<InventorySlotSync>
    {
        public int ItemId;
        public int Amount;
        public InventorySlotSync(int itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
        bool IEquatable<InventorySlotSync>.Equals(InventorySlotSync other)
        {
            return ItemId == other.ItemId && Amount == other.Amount;
        }
    }

    public class InventorySlot
    {
        public ItemData ItemData;
        public int Id => ItemData.Id;
        public ItemType Type => ItemData.Type;
        public Sprite Icon => ItemData.Icon;
        public int MaxStack => ItemData.MaxStack;
        public int Amount = 0;

        public InventorySlot(InventorySlotSync data) :
            this(ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(data.ItemId), ref data.Amount)
        {
        }
        public InventorySlot(ItemData data, ref int amount)
        {
            ItemData = data;
            Amount = Mathf.Min(amount, data.MaxStack);
            amount -= Amount;
        }

        public int Increase(int value)
        {
            int remainAmount = MaxStack - Amount;
            int trueValue = Mathf.Min(remainAmount, value);
            Amount += trueValue;
            return trueValue;
        }

        public int Decrease(int value)
        {
            int trueValue = Mathf.Min(Amount, value);
            Amount -= trueValue;
            return trueValue;
        }

        public bool IsEmpty() => Amount == 0;
    }
}
