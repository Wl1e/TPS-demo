using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class ShopSlotUI : MonoBehaviour
    {
        public Image Icon;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI PriceText;

        bool m_SeldOut;
        public bool SeldOut
        {
            get => m_SeldOut;
            set
            {
                m_SeldOut = value;
                if (m_SeldOut) {
                    OnSeldOut();
                }
            }
        }

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

        void OnSeldOut()
        {
            // 卖完了，图标变灰
        }
    }
}
