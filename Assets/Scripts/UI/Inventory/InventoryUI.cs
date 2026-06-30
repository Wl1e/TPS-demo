using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo.UI
{

using InventoryUpdateEvent = Event.InventoryUpdateEvent;
using InventoryStateChangeEvent = Event.InventoryStateChangeEvent;

    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] List<InventorySlotUI> m_Slots;
        bool m_IsOpened = false;

        void Awake()
        {
            EventManager.AddListener<InventoryUpdateEvent>(HandleUpdateUI);
            // 改成逻辑层接收输入，通过事件打开界面
            EventManager.AddListener<InventoryStateChangeEvent>(OnInventoryStateChange);
            for (int idx = 0; idx < m_Slots.Count; idx++) {
                m_Slots[idx].Inventory = this;
                m_Slots[idx].SlotIdx = idx;
            }
        }
        void OnDestroy()
        {
            EventManager.RemoveListener<InventoryUpdateEvent>(HandleUpdateUI);
            EventManager.RemoveListener<InventoryStateChangeEvent>(OnInventoryStateChange);
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

        void OnInventoryStateChange(Event.InventoryStateChangeEvent evt)
        {
            m_IsOpened = evt.IsOpened;
            gameObject.SetActive(m_IsOpened);
            Cursor.lockState = m_IsOpened ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
