using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class ShopSlotUI : MonoBehaviour, IPointerClickHandler
    {
        public Image Icon;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI PriceText;

        public event Action<ShopSlotUI> OnClick;

        bool m_SeldOut = false;
        public bool SeldOut => m_SeldOut;

        [HideInInspector] public int Price;
        [HideInInspector] public int FinalPrice;
        [HideInInspector] public float Discount;
        [HideInInspector] public int Amount;



        public void Initialize(string name, int price, float discount, int finalPrice, int amount, Sprite icon)
        {
            Price = price;
            FinalPrice = finalPrice;
            Amount = amount;
            Discount = discount;

            Name.text = name;
            PriceText.text = FinalPrice.ToString();
            Icon.sprite = icon;
        }

        public void OnSeldOut()
        {
            if(m_SeldOut) {
                return;
            }
            m_SeldOut = true;
            // 卖完了，图标变灰
            Icon.color = Color.Lerp(Icon.color, Color.black, 0.3f);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }
    }
}
