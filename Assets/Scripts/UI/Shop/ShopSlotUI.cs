
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
        public Image RestockMask;
        private float m_RestockTime;

        public event System.Action<ShopSlotUI> OnClick;
        public event System.Action<ShopSlotUI> RestockFinished;

        bool m_SeldOut = false;
        private bool m_Restocking = false;
        public bool SeldOut => m_SeldOut;
        public bool Restocking => m_Restocking;

        [HideInInspector] public int Price;
        [HideInInspector] public int FinalPrice;
        [HideInInspector] public float Discount;
        [HideInInspector] public int Amount;

        private Coroutine m_RestockCoroutine;

        public void Initialize(
            string name, int price,
            float discount, int finalPrice,
            int amount, Sprite icon, bool soldout,
            bool restocking, float restockTime
            )
        {
            Price = price;
            FinalPrice = finalPrice;
            Amount = amount;
            Discount = discount;

            Name.text = name;
            PriceText.text = FinalPrice.ToString();
            Icon.sprite = icon;

            UpdateState(soldout, restocking, restockTime);

            RestockMask.gameObject.SetActive(false);
        }

        public bool CanSell() => !m_SeldOut && !m_Restocking && m_RestockCoroutine == null;

        public void UpdateState(bool soldout, bool restock, float restockTime)
        {
            //print($"soldout: {soldout}, restock: {restock}, restockTime: {restockTime}");
            m_SeldOut = soldout;
            // 卖完了，图标变灰
            if (restock && m_RestockCoroutine == null) {
                m_RestockCoroutine = StartCoroutine(RestockCoroutine(restockTime));
            } else if(m_SeldOut) {
                Icon.color = Color.Lerp(Icon.color, Color.black, 0.5f);
            }
        }

        private System.Collections.IEnumerator RestockCoroutine(float time)
        {
            // 不对，直接把RestockFinished取消掉不就行了
            // 由于逻辑端的补货逻辑在Server上
            // 所以Shop里对应entry的补货标记晚于这里的更新
            // 所以RestockFinished会再触发一次补货界面
            // 给补货转圈多加0.5s，不是好方法，可能被网络延迟影响
            //time += 0.5f;
            print("开始补货转圈");
            m_Restocking = true;
            RestockMask.gameObject.SetActive(m_Restocking);
            RestockMask.fillAmount = 1;
            m_RestockTime = time;
            while (time > 0f) {
                time -= Time.deltaTime;
                RestockMask.fillAmount = time / m_RestockTime;
                yield return null;
            }
            m_Restocking = false;
            m_SeldOut = false;
            RestockMask.gameObject.SetActive(m_Restocking);
            //RestockFinished?.Invoke(this);
            m_RestockCoroutine = null;
            print("补货转圈完成");
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }
    }
}
