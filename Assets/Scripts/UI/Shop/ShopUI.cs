using Event;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ShopUI : MonoBehaviour
    {
        int m_CurrentShopId = 0;
        [SerializeField] Button m_CloseButton;

        [SerializeField] GameObject m_ShopSlotPrefab;
        [SerializeField] RectTransform m_SlotRoot;

        List<ShopSlotUI> m_Slots = new List<ShopSlotUI>();
        private void Start()
        {
            EventManager.AddListener<ShopOpenEvent>(OnShopOpen);
            EventManager.AddListener<ShopCloseEvent>(OnShopClose);
            EventManager.AddListener<ShopBuyEvent>(OnShopBuy);
            if (m_CloseButton) {
                m_CloseButton.onClick.AddListener(HandleCloseButtonClick);
            }
            gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            EventManager.RemoveListener<ShopOpenEvent>(OnShopOpen);
            EventManager.RemoveListener<ShopCloseEvent>(OnShopClose);
            EventManager.RemoveListener<ShopBuyEvent>(OnShopBuy);
            if (m_CloseButton) {
                m_CloseButton.onClick.RemoveListener(HandleCloseButtonClick);
            }
        }


        void OnShopOpen(ShopOpenEvent evt)
        {
            gameObject.SetActive(true);
            m_CurrentShopId = evt.ShopId;
            SetShopGoods(m_CurrentShopId);
        }

        void SetShopGoods(int shopId)
        {
            var goodsList = PlayerDataProxy.Instance.GetShopGoods(m_CurrentShopId);
            foreach (var entry in goodsList) {
                AddSlot(entry);
            }
        }

        void AddSlot(ShopEntry entry)
        {
            var slotObj = Instantiate(m_ShopSlotPrefab, m_SlotRoot);
            var slot = slotObj.GetComponent<ShopSlotUI>();
            slot.Initialize(entry.ItemName, entry.Price, entry.Discount,
                entry.FinalPrice, entry.Amount, ItemUIUtils.GetItemSprite(entry.ItemId));
            m_Slots.Add(slot);
        }

        void OnShopClose(ShopCloseEvent evt)
        {
            m_Slots.Clear();
            for (int idx = m_SlotRoot.childCount - 1; idx >= 0; idx--) {
                Destroy(m_SlotRoot.GetChild(idx).gameObject);
            }
            gameObject.SetActive(false);
        }
        void OnShopBuy(ShopBuyEvent evt)
        {

        }

        void HandleCloseButtonClick()
        {
            EventManager.Broadcast(new CloseShopEvent());
        }
    }
}
