using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo.UI
{

using InventoryUpdateEvent = Event.InventoryUpdateEvent;
using InventoryStateChangeEvent = Event.InventoryStateChangeEvent;

    public class InventoryUI : MonoBehaviour, IPanel
    {
        [SerializeField] List<InventorySlotUI> m_Slots;
        bool m_IsOpened = false;

        // 装备槽在UI上是背包的一部分，为了降低Inventory复杂度，单独拿出来
        // 所以当背包打开/关闭时，装备槽也要跟着显示/隐藏
        [SerializeField] LoadoutUI m_Loadout; 

        void Awake()
        {
            EventManager.AddListener<InventoryUpdateEvent>(HandleUpdateUI);
            for (int idx = 0; idx < m_Slots.Count; idx++) {
                m_Slots[idx].Inventory = this;
                m_Slots[idx].SlotIdx = idx;
            }
        }
        void OnDestroy()
        {
            EventManager.RemoveListener<InventoryUpdateEvent>(HandleUpdateUI);
        }

        private void Start()
        {
            gameObject.SetActive(m_IsOpened);
        }

        public void Initialize()
        {
            UpdateUI(PlayerDataProxy.Instance.GetInventoryData());
        }

        void HandleUpdateUI(InventoryUpdateEvent evt)
        {
            UpdateUI(PlayerDataProxy.Instance.GetInventoryData());
        }

        void UpdateUI(List<(int ItemId, int Amount)> data)
        {
            for (int i = 0; i < m_Slots.Count; i++) {
                if (data[i].ItemId > 0) {
                    if (!m_Slots[i].Item) {
                        m_Slots[i].InitializeItem();
                    }
                    m_Slots[i].Item.UpdateIconAndAmount(data[i].ItemId, data[i].Amount);
                } else {
                    m_Slots[i].DestroyItem();
                }
            }
        }

        public void Swap(int slotIdx1, int slotIdx2)
        {
            EventManager.Broadcast(
                new Event.InventoryTrySwapItemEvent {
                    SlotIdx1 = slotIdx1, SlotIdx2 = slotIdx2
                }
            );
        }

        public void Open()
        {
            m_IsOpened = true;
            gameObject.SetActive(m_IsOpened);
            m_Loadout.gameObject.SetActive(m_IsOpened);
        }

        public void Close()
        {
            m_IsOpened = false;
            gameObject.SetActive(m_IsOpened);
            m_Loadout.gameObject.SetActive(m_IsOpened);
        }
    }
}
