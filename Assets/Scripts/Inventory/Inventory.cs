
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace TPSDemo
{

    public class Inventory : MonoBehaviour
    {
        List<InventorySlot> m_Items = new List<InventorySlot>();
        int m_Size;
        public int DefaultSize = 12;

        bool m_IsOpened = false;

        [SerializeField] GameEvent m_InventoryEvent;

        PlayerController m_Owner;

        public event Action OnInventoryUpdate;

        void Awake()
        {
            Resize(DefaultSize);
        }

        private void OnEnable()
        {
            EventManager.AddListener<Event.InventoryTrySwapItem>(SwapItem);
            m_InventoryEvent.RegisterListener(OnInventoryInput);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<Event.InventoryTrySwapItem>(SwapItem);
            m_InventoryEvent.UnregisterListener(OnInventoryInput);
        }

        private void Start()
        {
            m_Owner = GetComponent<PlayerController>();
        }

        int FindFirstEmptySlot()
        {
            return m_Items.FindIndex(0, item => item == null);
        }
        int FindLastTypeSlot(ItemType type)
        {
            return m_Items.FindLastIndex(item => item.Type == type);
        }
        public int GetAmount(int itemId)
        {
            int amount = 0;
            m_Items.ForEach(item => amount += item?.Id == itemId ? item.Amount : 0);
            return amount;
        }
        public List<(int itemId, int amount)> GetItems(ItemType type)
        {
            List<(int, int)> result = new List<(int, int)>();
            foreach (var item in m_Items) {
                if (item != null && item.Type == type) {
                    result.Add((item.Id, item.Amount));
                }
            }

            return result;
        }

        public List<(int itemId, int amount)> GetAllItem()
        {
            List<(int, int)> result = new List<(int, int)>();
            foreach (var item in m_Items) {
                if (item == null) {
                    result.Add((0, 0));
                } else {
                    result.Add((item.Id, item.Amount));
                }
            }

            return result;
        }

        public ItemData GetFirstSlotData(int itemId)
        {
            return m_Items.Find(slot => slot.Id == itemId)?.ItemData;
        }

        bool ValidIndex(int index)
        {
            return index >= 0 && index < m_Size;
        }

        public bool AddItem(IItem item, int idx)
        {
            if (m_Items[idx] == null) {
                int amount = item is IStackable stackable ? stackable.Amount : 1;
                m_Items[idx] = new InventorySlot(item, ref amount);
                UpdateInventory();
                return true;
            }
            return false;
        }

        public bool AddItem(int itemId, int amount)
        {
            for (int i = 0; i < m_Items.Count; i++) {
                if (amount <= 0) {
                    break;
                }
                var item = m_Items[i];
                if (item != null && item.Id == itemId) {
                    amount -= item.Increase(amount);
                }
                if (item == null) {
                    m_Items[i] = new InventorySlot(
                        ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(itemId),
                        ref amount
                    );
                }
            }
            if (amount > 0) {
                //DropItem(ItemDataList.GetItemData(itemId), amount);
            }
            UpdateInventory();
            return true;
        }

        public bool AddItem(IItem item)
        {
            int amount = item is IStackable stackable ? stackable.Amount : 1;
            print($"Inventory Add Item: {item.Name}, amount: {amount}");
            int tmp = amount;
            for (int i = 0; i < m_Size; i++) {
                if (amount <= 0) {
                    break;
                }
                if (m_Items[i] != null && m_Items[i].Id == item.Id) {
                    amount -= m_Items[i].Increase(amount);
                }
            }
            if (amount > 0) {
                int idx = FindFirstEmptySlot();
                if (idx != -1) {
                    m_Items[idx] = new InventorySlot(item, ref amount);
                }
            }
            UpdateInventory();
            return amount < tmp;
        }

        public void Sort()
        {
            UpdateInventory();
        }

        public void SwapItem(Event.InventoryTrySwapItem evt)
        {
            int idx1 = evt.SlotIdx1;
            int idx2 = evt.SlotIdx2;
            Assert.IsTrue(ValidIndex(idx1));
            Assert.IsTrue(ValidIndex(idx2));
            var item1 = m_Items[idx1];
            var item2 = m_Items[idx2];
            m_Items[idx1] = item2;
            m_Items[idx2] = item1;
            UpdateInventory();
        }

        public void RemoveItem(int idx, bool drop = false)
        {
            Assert.IsTrue(ValidIndex(idx));
            if (drop) {
                Drop(idx);
            }
            m_Items[idx] = null;
            UpdateInventory();
        }

        public int ReduceItemAmount(int itemId, int amount = 1)
        {
            if (GetAmount(itemId) < amount) {
                return 0;
            }
            int remainAmount = amount;
            for (int idx = m_Size - 1; idx >= 0; idx--) {
                if (m_Items[idx] != null && m_Items[idx].Id == itemId) {
                    remainAmount -= m_Items[idx].Decrease(remainAmount);
                    if (m_Items[idx].IsEmpty()) {
                        m_Items[idx] = null;
                    }
                    if (remainAmount <= 0) {
                        break;
                    }
                }
            }
            UpdateInventory();
            return amount - remainAmount;
        }

        void Resize(int size)
        {
            bool needUpdate = false;
            m_Size = size;
            for (int i = m_Size; i < m_Items.Count; i++) {
                Drop(i);
                needUpdate = true;
            }
            while (m_Items.Count < m_Size) {
                m_Items.Add(null);

            }
            if (needUpdate) {
                UpdateInventory();
            }
        }

        void Drop(int idx)
        {
            //DropItem(m_Items[idx].ItemData, m_Items[idx].Amount);
            m_Items[idx] = null;
        }



        // 基本上更新都和Slot的修改有关，直接给Slot加一个ChangedAction，然后让Inventory监听修改直接更新？
        void UpdateInventory()
        {
            OnInventoryUpdate?.Invoke();
            EventManager.Broadcast(new Event.InventoryUpdateEvent());
        }

        void OnInventoryInput()
        {
            m_IsOpened = !m_IsOpened;
            if (m_IsOpened) {
                m_Owner.SetInputActive(false, false);
            } else {
                m_Owner.SetInputActive(true, true);
            }
            EventManager.Broadcast(new Event.InventoryStateChangeEvent { IsOpened = m_IsOpened });
        }
    }
}
