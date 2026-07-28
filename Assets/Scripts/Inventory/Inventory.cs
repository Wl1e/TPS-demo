
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace TPSDemo
{
    public class Inventory : NetworkBehaviour
    {
        private readonly List<InventorySlot> m_Items = new();
        private int m_Size;
        public int DefaultSize = 12;
        public readonly NetworkVariable<Vector2Int> SyncValue = new(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner
        );

        [SerializeField] private GameEvent m_InventoryEvent;

        private PlayerController m_Owner;

        public event Action OnInventoryUpdate;

        private Vector3 DropPos() => transform.position + Vector3.up * 0.1f;

        private void Awake()
        {
            m_Owner = GetComponent<PlayerController>();
            Resize(DefaultSize);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner) {
                EventManager.AddListener<Event.InventoryTrySwapItemEvent>(SwapItem);
                EventManager.AddListener<Event.InventoryDropItemEvent>(DropItem);
                m_InventoryEvent.RegisterListener(OnInventoryInput);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) {
                EventManager.RemoveListener<Event.InventoryTrySwapItemEvent>(SwapItem);
                EventManager.RemoveListener<Event.InventoryDropItemEvent>(DropItem);
                m_InventoryEvent.UnregisterListener(OnInventoryInput);
            }
            base.OnNetworkDespawn();
        }

        public InventorySlot GetItem(int slotIdx) => m_Items[slotIdx];

        private int FindFirstEmptySlot()
        {
            return m_Items.FindIndex(0, item => item == null);
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

        public ItemData GetFirstSlotData(int itemId) => m_Items.Find(slot => slot.Id == itemId)?.ItemData;

        private bool ValidIndex(int index) => index >= 0 && index < m_Size;

        public bool AddItem(IItem item, int idx)
        {
            if (m_Items[idx] == null) {
                int amount = item is IStackable stackable ? stackable.Amount : 1;
                m_Items[idx] = new InventorySlot(item.Data, ref amount);
                UpdateInventory();
                return true;
            }
            return false;
        }

        public bool AddItem(int itemId, int amount)
        {
            Debug.Log($"AddItem {itemId} {amount}");
            for (int i = 0; i < m_Size; i++) {
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
                DropItemServerRpc(itemId, amount, DropPos());
            }
            UpdateInventory();
            return true;
        }

        public bool AddItem(IItem item)
        {
            int amount = item is IStackable stackable ? stackable.Amount : 1;
            Debug.Log($"Inventory Add Item: {item.Name}, amount: {amount}");
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
                    m_Items[idx] = new InventorySlot(item.Data, ref amount);
                }
            }
            UpdateInventory();
            return amount < tmp;
        }

        public void Sort() => UpdateInventory();

        public void SwapItem(Event.InventoryTrySwapItemEvent evt)
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
            SyncValue.Value = new(itemId, amount - remainAmount);
            return amount - remainAmount;
        }

        [Rpc(SendTo.Owner)]
        public void ReduceItemAmountRpc(int itemId, int amount) => ReduceItemAmount(itemId, amount);

        private void Resize(int size)
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

        private void Drop(int idx)
        {
            //DropItem(m_Items[idx].ItemData, m_Items[idx].Amount);
            var itemData = GetItem(idx).ItemData;
            int amount = GetItem(idx).Amount;
            m_Items[idx] = null;
            DropItemServerRpc(itemData.Id, amount, DropPos());
            UpdateInventory();
        }

        private void DropItem(Event.InventoryDropItemEvent evt) => Drop(evt.Slot);

        // 基本上更新都和Slot的修改有关，直接给Slot加一个ChangedAction，然后让Inventory监听修改直接更新？
        private void UpdateInventory()
        {
            OnInventoryUpdate?.Invoke();
            EventManager.Broadcast(new Event.InventoryUpdateEvent());
        }

        private void OnInventoryInput() => EventManager.Broadcast(new Event.InventoryStateChangeEvent());

        #region Server

        [ClientRpc]
        public void AddItemClientRpc(int itemId, int amount)
        {
            if(!IsOwner) {
                return;
            }
            AddItem(itemId, amount);
        }

        [ClientRpc]
        public void RemoveItemClientRpc(int playerId, int slot)
        {
            if (IsOwner) {
                return;
            }
            RemoveItem(slot);
        }

        [ServerRpc]
        public void DropItemServerRpc(int itemId, int amount, Vector3 playerPos)
        {
            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(itemId);
            StartCoroutine(WorldItemManager.Instance.SpawnItem(itemData, playerPos, amount));
        }

        #endregion
    }
}
