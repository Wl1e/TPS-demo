
using UnityEngine;
using UnityEngine.EventSystems;

namespace TPSDemo
{
    public class InventorySlot
    {
        public ItemData ItemData;
        public int Id => ItemData.Id;
        public ItemType Type => ItemData.Type;
        public Sprite Icon => ItemData.Icon;
        public int MaxStack => ItemData.MaxStack;
        public int Amount = 0;

        public bool Draging = false;

        public InventorySlot(IItem item, ref int amount) : this(item?.Data, ref amount)
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

        public bool IsEmpty()
        {
            return Amount == 0;
        }
    }
}
