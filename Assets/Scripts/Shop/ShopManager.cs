using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] ShopList m_ShopConfigList;
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

        foreach (var config in m_ShopConfigList.Configs) {
            var shopGO = new GameObject("Shop" + config.ShopId, typeof(Shop));
            shopGO.transform.SetParent(shopRoot.transform, false);
            var shop = shopGO.GetComponent<Shop>();
            shop.Initialize(config);
            m_Shops.Add(shop.ShopId, shop);
        }

    }

    void OnOpenShop(Event.OpenShopEvent evt)
    {
        OpenShop(evt.ShopId, evt.playerId);
        m_CurrentShopId = evt.ShopId;
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

    public List<ShopEntry> GetGoods(int shopId)
    {
        return m_Shops.TryGetValue(shopId, out Shop shop) ? shop.Goods : new List<ShopEntry>();
    }

    public void Register(Shop shop)
    {
        if (m_Shops.ContainsKey(shop.ShopId)) return;
        m_Shops[shop.ShopId] = shop;
    }

    public void Save()
    {
    }

    void OnDestroy()
    {
        Save();
    }
}
