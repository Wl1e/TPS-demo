using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    using Event;
    using TMPro;
    using TPSDemo.Combat;
    using WebSocketSharp;

    public class ShopUI : MonoBehaviour, IPanel
    {
        int m_CurrentShopId = -1;
        [SerializeField] Button m_CloseButton;

        [SerializeField] ShopSlotUI m_ShopSlotPrefab;
        [SerializeField] RectTransform m_SlotRoot;

        [SerializeField] TextMeshProUGUI m_Money;

        private readonly List<ShopSlotUI> m_Slots = new();
        private void Start()
        {
            EventManager.AddListener<ShopOpenEvent>(OnShopOpen);
            EventManager.AddListener<ShopCloseEvent>(OnShopClose);
            EventManager.AddListener<ShopBuyEvent>(OnShopBuy);
            EventManager.AddListener<PlayerEconomyChangedEvent>(OnEconomyChanged);
            EventManager.AddListener<ShopUpdateEvent>(OnShopUpdated);
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
            EventManager.RemoveListener<ShopUpdateEvent>(OnShopUpdated);
            if (m_CloseButton) {
                m_CloseButton.onClick.RemoveListener(HandleCloseButtonClick);
            }
        }


        void OnShopOpen(ShopOpenEvent evt)
        {
            m_CurrentShopId = evt.ShopId;
            m_Money.text = PlayerDataProxy.Instance.GetMoney(m_CurrentShopId).ToString();
            SetShopGoods(evt.ShopId);
        }

        void SetShopGoods(int shopId)
        {
            var goods = PlayerDataProxy.Instance.GetShopGoods(shopId);
            for(int i = 0; i < goods.Count; i++) {
                SetSlot(i, goods[i]);
            }
        }

        void SetSlot(int slotIdx, ShopEntry entry)
        {
            print($"entry name: {entry.GoodName}, soldout: {entry.Soldout}");
            ShopSlotUI slot = null;
            while (m_Slots.Count <= slotIdx) {
                slot = Instantiate(m_ShopSlotPrefab, m_SlotRoot);
                slot.OnClick += self => {
                    if (self.SeldOut) {
                        EventManager.Broadcast(new MessageLogEvent { Message = $"商品{self.Name.text}已售空" });
                        return;
                    }
                    EventManager.Broadcast(new TryBuyEvent { ShopId = m_CurrentShopId, Slot = m_Slots.IndexOf(self) });
                };

                m_Slots.Add(slot);
            }
            slot = m_Slots[slotIdx];

            slot.Initialize(
                entry.GoodName,
                entry.Price,
                entry.Discount,
                entry.FinalPrice,
                entry.Amount,
                ItemUIUtils.GetItemIcon(entry.GoodId),
                entry.Soldout
            );
        }

        void OnShopClose(ShopCloseEvent evt)
        {
            m_Slots.Clear();
            for (int idx = m_SlotRoot.childCount - 1; idx >= 0; idx--) {
                Destroy(m_SlotRoot.GetChild(idx).gameObject);
            }
            m_CurrentShopId = -1;
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
            if(m_CurrentShopId != -1 && evt.MoneyId == ResourceManager.Instance.GetResource<ShopList>("Shop").GetConfig(m_CurrentShopId).MoneyId) {
                m_Money.text = evt.Amount.ToString();
            }
        }

        private void OnShopUpdated(ShopUpdateEvent evt)
        {
            if (evt.ShopId != m_CurrentShopId) {
                return;
            }
            SetShopGoods(m_CurrentShopId);
        }

        void HandleCloseButtonClick()
        {
            EventManager.Broadcast(new CloseShopEvent());
        }

        void IPanel.Open()
        {
            gameObject.SetActive(true);
        }

        void IPanel.Close()
        {
            gameObject.SetActive(false);
        }
    }
}
