using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    public class ShopManager : Singleton<ShopManager>
    {
        Dictionary<int, Shop> m_Shops = new Dictionary<int, Shop>();

        int m_CurrentShopId;
        int m_CurrentPlayerId;

        void Start()
        {
            InitializeAllShop();
        }

        private void OnEnable()
        {
            EventManager.AddListener<Event.OpenShopEvent>(OnOpenShop);
            EventManager.AddListener<Event.CloseShopEvent>(OnCloseShop);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Event.OpenShopEvent>(OnOpenShop);
            EventManager.RemoveListener<Event.CloseShopEvent>(OnCloseShop);
        }

        void InitializeAllShop()
        {
            var shopRoot = new GameObject("ShopRoot");

            var shopConfig = ResourceManager.Instance.GetResource<ShopList>("Shop");
            foreach (var config in shopConfig.Configs) {
                var shopGO = new GameObject("Shop" + config.ShopId, typeof(Shop));
                shopGO.transform.SetParent(shopRoot.transform, false);
                var shop = shopGO.GetComponent<Shop>();
                shop.Initialize(config);
                Register(shop);
            }

        }

        void OnOpenShop(Event.OpenShopEvent evt)
        {
            m_CurrentShopId = evt.ShopId;
            OpenShop(m_CurrentShopId, evt.playerId);
        }

        void OpenShop(int shopId, int playerId)
        {
            if (m_Shops.TryGetValue(shopId, out Shop shop)) {
                m_CurrentPlayerId = playerId;
                var playerActor = ActorManager.Instance.GetActor(m_CurrentPlayerId);
                shop.Enter(playerActor.GetComponent<PlayerController>());
            }
        }

        void OnCloseShop(Event.CloseShopEvent evt)
        {
            if (m_Shops.TryGetValue(m_CurrentShopId, out Shop shop)) {
                var playerActor = ActorManager.Instance.GetActor(m_CurrentPlayerId);
                shop.Exit(playerActor.GetComponent<PlayerController>());
            }
        }

        public Shop GetCurrentShop()
        {
            return m_Shops.GetValueOrDefault(m_CurrentShopId, null);
        }

        public void Register(Shop shop)
        {
            if (m_Shops.ContainsKey(shop.ShopId))
                return;
            m_Shops.Add(shop.ShopId, shop);
        }

        public void Save()
        {
        }

        void OnDestroy()
        {
            Save();
        }
    }
}
