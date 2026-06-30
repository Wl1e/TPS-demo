using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using TPSDemo.UI;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class ShopManager : NetworkSingleton<ShopManager>
    {
        private readonly Dictionary<int, Shop> m_Shops = new();
        /// <summary>
        /// 本地打开Shop的Id
        /// </summary>
        private int m_CurrentShopId;
        /// <summary>
        /// 本地打开Shop的Player的Id
        /// </summary>
        private int m_CurrentPlayerId;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            InitializeAllShop();
            if(IsClient) {
                EventManager.AddListener<Event.OpenShopEvent>(OnOpenShop);
                EventManager.AddListener<Event.CloseShopEvent>(OnCloseShop);
                EventManager.AddListener<Event.TryBuyEvent>(TryBuy);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient) {
                EventManager.RemoveListener<Event.OpenShopEvent>(OnOpenShop);
                EventManager.RemoveListener<Event.CloseShopEvent>(OnCloseShop);
                EventManager.RemoveListener<Event.TryBuyEvent>(TryBuy);
            }
            base.OnNetworkDespawn();
        }

        private bool HasShop(int shopId) => m_Shops.ContainsKey(shopId);

        void InitializeAllShop()
        {
            var shopConfig = ResourceManager.Instance.GetResource<ShopList>("Shop");
            foreach (var config in shopConfig.Configs) {
                if (m_Shops.ContainsKey(config.ShopId)) {
                    return;
                }
                m_Shops.Add(config.ShopId, new Shop(config));
            }
        }

        #region ShopBuy

        public void TryBuy(Event.TryBuyEvent evt) => TryBuyServerRpc(m_CurrentPlayerId, evt.ShopId, evt.Slot);

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void TryBuyServerRpc(int playerId, int shopId, int slot)
        {
            print($"actorId: {playerId}");
            var player = ActorManager.Instance.GetActor(playerId).GetComponent<PlayerController>();
            var shop = m_Shops[shopId];
            var evt = shop.Buy(player, slot);
            BuyResultClientRpc(playerId, evt);
            if (evt.IsSuccess) {
                UpdateShopGoodsClientRpc(shopId, shop.GoodArr);
            }
        }

        [ClientRpc]
        private void BuyResultClientRpc(int playerId, Event.ShopBuyEvent evt)
        {
            if(m_CurrentPlayerId != playerId) {
                return;
            }

            EventManager.Broadcast(evt);

            var itemName = m_Shops[evt.ShopId].GetGood(evt.Slot).Value.GoodName;
            if (evt.IsSuccess) {
                EventManager.Broadcast(
                    new Event.MessageLogEvent {
                        Message = $"Player{playerId} 购买{itemName}成功"
                    }
                );
            } else {
                EventManager.Broadcast(
                    new Event.MessageLogEvent {
                        Message = $"Player{playerId} 购买{itemName}失败，金币不足"
                    }
                );
            }
        }

        #endregion

        #region ShopOpen

        void OnOpenShop(Event.OpenShopEvent evt)
        {
            m_CurrentPlayerId = evt.playerId;
            print("Enter Shop, Player " + m_CurrentPlayerId);
            m_CurrentShopId = evt.ShopId;
            OpenShopServerRpc(m_CurrentPlayerId, m_CurrentShopId);
        }

        // 我使用了PlayerId来锁定客户端，强要求：Client中，只有Owner Player才允许打开Shop
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void OpenShopServerRpc(int playerId, int shopId)
        {
            if(!HasShop(shopId)) {
                return;
            }
            UpdateShopGoodsClientRpc(shopId, m_Shops[shopId].GoodArr);
            TrueEnterShopClientRpc(playerId);
        }

        [ClientRpc]
        private void TrueEnterShopClientRpc(int playerId)
        {
            if (playerId == m_CurrentPlayerId && m_Shops.TryGetValue(m_CurrentShopId, out Shop shop)) {
                var player = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
                // 如果后续还有根据PlayerId找Player的需求，可以考虑在本地映射自己的player
                //var playerActor = ActorManager.Instance.GetActor(m_CurrentPlayerId);
                print($"player: {player}, shop: {shop}");
                shop.Enter(player);
            }
        }

        void OnCloseShop(Event.CloseShopEvent evt)
        {
            if (m_Shops.TryGetValue(m_CurrentShopId, out Shop shop)) {
                var player = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
                shop.Exit(player);
            }
        }

        #endregion

        #region ShopUpdate

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void UpdateShopServerRpc(int shopId) => UpdateShopGoodsClientRpc(shopId, m_Shops[shopId].GoodArr);

        // 每次更新都做了List和Array的转换，有性能开销吗
        [ClientRpc]
        private void UpdateShopGoodsClientRpc(int shopId, ShopEntry[] goods)
        {
            var shop = m_Shops[shopId];
            shop.SetGoods(goods);
            EventManager.Broadcast(new Event.ShopUpdateEvent { ShopId = shopId });
        }

        public Shop GetCurrentShop()
        {
            return m_Shops.GetValueOrDefault(m_CurrentShopId, null);
        }

        #endregion

        public void Save()
        {
        }
    }
}
