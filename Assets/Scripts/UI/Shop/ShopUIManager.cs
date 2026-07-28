using System.Collections.Generic;
using TPSDemo.Event;
using UnityEngine;

namespace TPSDemo.UI
{
    /// <summary>
    /// 控制打开哪个商店界面
    /// </summary>
    public class ShopUIManager : MonoBehaviour, IPanel
    {
        private readonly Dictionary<int, ShopUI> m_ShopUI = new();
        private int m_CurrentShopId = -1;

        private void Start()
        {
            EventManager.AddListener<OpenShopUIEvent>(OnShopOpen);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<OpenShopUIEvent>(OnShopOpen);
        }

        private void OnShopOpen(OpenShopUIEvent evt)
        {
            if (!m_ShopUI.ContainsKey(evt.ShopId)) {
                CreateShopUI(evt.ShopId);
            }
            m_CurrentShopId = evt.ShopId;
            var shop = m_ShopUI[m_CurrentShopId];
            Debug.Log("打开" + shop.gameObject);
            shop.gameObject.SetActive(true);
            shop.OnOpen();
        }

        private void CreateShopUI(int shopId)
        {
            var shopConfig = ResourceManager.Instance.GetResource<ShopList>("Shop").GetConfig(shopId);
            if (shopConfig == null) {
                Debug.LogError($"不存在Shop {shopId}");
                EventManager.Broadcast(new MessageLogEvent { Message = "不存在该商店" });
                return;
            }
            var shopObj = Instantiate(shopConfig.ShopUI);
            shopObj.transform.SetParent(transform, false);
            m_ShopUI[shopId] = shopObj.GetComponent<ShopUI>();
            shopObj.SetActive(false);
        }

        // shopmanager是个特例
        void IPanel.Open()
        {
            //var shop = m_ShopUI[m_CurrentShopId];
            //shop.gameObject.SetActive(true);
        }

        void IPanel.Close()
        {
            m_ShopUI[m_CurrentShopId].gameObject.SetActive(false);
            m_CurrentShopId = -1;
        }
    }
}
