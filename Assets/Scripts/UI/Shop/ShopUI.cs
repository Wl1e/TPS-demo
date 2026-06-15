using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    using Event;
    using TMPro;
    using TPSDemo.Combat;

    public class ShopUI : MonoBehaviour
    {
        int m_CurrentShopId = 1;
        [SerializeField] Button m_CloseButton;

        [SerializeField] GameObject m_ShopSlotPrefab;
        [SerializeField] RectTransform m_SlotRoot;

        [SerializeField] TextMeshProUGUI m_Money;

        private readonly List<ShopSlotUI> m_Slots = new();
        private void Start()
        {
            EventManager.AddListener<ShopOpenEvent>(OnShopOpen);
            EventManager.AddListener<ShopCloseEvent>(OnShopClose);
            EventManager.AddListener<ShopBuyEvent>(OnShopBuy);
            EventManager.AddListener<PlayerEconomyChangedEvent>(OnEconomyChanged);
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
            EventManager.RemoveListener<PlayerEconomyChangedEvent>(OnEconomyChanged);
            if (m_CloseButton) {
                m_CloseButton.onClick.RemoveListener(HandleCloseButtonClick);
            }
        }


        void OnShopOpen(ShopOpenEvent evt)
        {
            gameObject.SetActive(true);
            m_CurrentShopId = evt.ShopId;
            m_Money.text = PlayerDataProxy.Instance.GetMoney(m_CurrentShopId).ToString();
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
            slot.Initialize(
                entry.ItemName,
                entry.Price,
                entry.Discount,
                entry.FinalPrice,
                entry.Amount,
                ItemUIUtils.GetItemSprite(entry.ItemId)
            );
            m_Slots.Add(slot);
            slot.OnClick += (ShopSlotUI slot) => {
                if(slot.SeldOut) {
                    return;
                }
                EventManager.Broadcast(new TryBuyEvent { Slot = m_Slots.IndexOf(slot) });
            };
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
            if(evt.ShopId != m_CurrentShopId) {
                return;
            }
            var slot = m_Slots[evt.Slot];
            slot.OnSeldOut();
        }

        private void OnEconomyChanged(PlayerEconomyChangedEvent evt)
        {
            if(evt.MoneyId == ResourceManager.Instance.GetResource<ShopList>("Shop").GetConfig(m_CurrentShopId).MoneyId) {
                m_Money.text = evt.Amount.ToString();
            }
        }

        void HandleCloseButtonClick()
        {
            EventManager.Broadcast(new CloseShopEvent());
        }
    }
}
