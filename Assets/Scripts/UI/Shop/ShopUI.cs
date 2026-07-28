using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    using Event;
    using TMPro;

    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private int m_ShopId;
        [SerializeField] private Button m_CloseButton;

        [SerializeField] private ShopSlotUI m_ShopSlotPrefab;
        [SerializeField] private RectTransform m_SlotRoot;

        [SerializeField] private TextMeshProUGUI m_Money;

        private readonly List<ShopSlotUI> m_Slots = new();

        public delegate void CloseSelf();

        private void Start()
        {
            //EventManager.AddListener<OpenShopUIEvent>(OnShopOpen);
            //EventManager.AddListener<CloseShopUIEvent>(OnShopClose);
            EventManager.AddListener<ShopBuyEvent>(OnShopBuy);
            EventManager.AddListener<PlayerEconomyChangedEvent>(OnEconomyChanged);
            EventManager.AddListener<ShopUpdateEvent>(OnShopUpdated);
            if (m_CloseButton) {
                m_CloseButton.onClick.AddListener(HandleCloseButtonClick);
            }
        }

        private void OnDestroy()
        {
            //EventManager.RemoveListener<OpenShopUIEvent>(OnShopOpen);
            //EventManager.RemoveListener<CloseShopUIEvent>(OnShopClose);
            EventManager.RemoveListener<ShopBuyEvent>(OnShopBuy);
            EventManager.RemoveListener<PlayerEconomyChangedEvent>(OnEconomyChanged);
            EventManager.RemoveListener<ShopUpdateEvent>(OnShopUpdated);
            if (m_CloseButton) {
                m_CloseButton.onClick.RemoveListener(HandleCloseButtonClick);
            }
        }

        private void OnShopBuy(ShopBuyEvent evt)
        { }

        private void UpdateGoodState(int shopId, int slot)
        {
            var goods = PlayerDataProxy.Instance.GetShopGoods(shopId);
            if (slot >= goods.Count) {
                return;
            }
            var good = goods[slot];
            var uiSlot = m_Slots[slot];

            uiSlot.UpdateState(good.Soldout, good.Restocking, good.RestockTime);
        }

        void SetSlot(int slotIdx, ShopEntry entry)
        {
            ShopSlotUI slot = null;
            while (m_Slots.Count <= slotIdx) {
                slot = Instantiate(m_ShopSlotPrefab, m_SlotRoot);
                m_Slots.Add(slot);
                slot.OnClick += self => {
                    //if (self.SeldOut || self.Restocking) {
                    //    EventManager.Broadcast(new MessageLogEvent { Message = $"商品{self.Name.text}已售空" });
                    //    return;
                    //}
                    EventManager.Broadcast(new TryBuyEvent { ShopId = m_ShopId, Slot = m_Slots.IndexOf(self) });
                };
                var idx = m_Slots.Count - 1;
                slot.RestockFinished += self => UpdateGoodState(
                    m_ShopId, idx
                );

            }
            slot = m_Slots[slotIdx];

            slot.Initialize(
                entry.GoodName,
                entry.Price,
                entry.Discount,
                entry.FinalPrice,
                entry.Amount,
                ItemUIUtils.GetItemIcon(entry.GoodId),
                entry.Soldout,
                entry.Restocking,
                entry.RestockTime
            );
        }

        private void OnEconomyChanged(PlayerEconomyChangedEvent evt)
        {
            if(m_ShopId != -1 && evt.MoneyId == ResourceManager.Instance.GetResource<ShopList>("Shop").GetConfig(m_ShopId).MoneyId) {
                m_Money.text = evt.Amount.ToString();
            }
        }

        private void OnShopUpdated(ShopUpdateEvent evt)
        {
            if (evt.ShopId != m_ShopId) {
                return;
            }
            UpdateGoodState(m_ShopId, evt.Slot);
        }

        void HandleCloseButtonClick()
        {
            EventManager.Broadcast(new CloseShopEvent());
        }

        public void OnOpen()
        {
            m_Money.text = PlayerDataProxy.Instance.GetMoney(m_ShopId).ToString();
            var goods = PlayerDataProxy.Instance.GetShopGoods(m_ShopId);
            for (int i = 0; i < goods.Count; i++) {
                SetSlot(i, goods[i]);
            }
        }
    }
}
