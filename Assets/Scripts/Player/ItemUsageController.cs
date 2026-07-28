using Unity.Netcode;
using UnityEngine;
using WebSocketSharp;

namespace TPSDemo
{

    public class ItemUsageController : NetworkBehaviour
    {
        private GameObject m_Item = null;
        private IActiveItem m_CurrentActiveItem = null;
        private int m_ItemId = -1;
        private float m_RemainTime = 0f;
        private bool m_Using = false;
        private Inventory m_Inventory;
        private PlayerRuntimeData m_PlayerRuntimeData;

        [SerializeField] private Transform Hand;

        private void Awake()
        {
            // 访问背包是一个很常见的需求，可以考虑再黑板或Player上加个方法
            var player = GetComponent<PlayerController>();
            m_Inventory = player.Inventory;
            m_PlayerRuntimeData = player.RuntimeData;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) {
                EventManager.AddListener<Event.TryUseActiveItemEvent>(UseActiveItem);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) {
                EventManager.RemoveListener<Event.TryUseActiveItemEvent>(UseActiveItem);
            }
        }

        #region StartUse

        private void UseActiveItem(Event.TryUseActiveItemEvent evt)
        {
            var itemData = m_Inventory.GetItem(evt.InventorySlotId).ItemData;
            OnUseActiveItemRpc(itemData.Id);

            StartCoroutine(AssetCache.GetOrLoad(
                itemData.Prefab, obj => {
                    m_Item = obj;
                    m_ItemId = itemData.Id;
                    m_Item.transform.SetParent(Hand, false);
                    StartUse(m_Item.GetComponent<IActiveItem>());
                })
            );
        }

        public void StartUse(IActiveItem item)
        {
            m_CurrentActiveItem = item;
            m_RemainTime = m_CurrentActiveItem.UseTime;
            m_Using = true;
            m_PlayerRuntimeData.UsingActiveItem = true;
            m_PlayerRuntimeData.AniParameter.UseActiveItem = true;
            m_PlayerRuntimeData.AniParameter.UseTime = item.UseTime;
        }

        //[ServerRpc]
        //private void OnUseActiveItemServerRpc(int itemId) => OnUseActiveItemClientRpc(itemId);

        #endregion

        private void Update()
        {
            if(!IsOwner) {
                return;
            }
            if (!m_Using || m_CurrentActiveItem == null) {
                return;
            }

            if (m_RemainTime > 0f) {
                m_RemainTime -= Time.deltaTime;
            } else {
                EndUse();
            }
        }

        #region EndUse
        public void StopUse()
        {
            if(m_Using) {
                EndUseRpc();
                ResetVariables();
            }
        }

        private void ResetVariables()
        {
            print("ResetVariables");
            m_Using = false;
            if(m_Item != null) {
                print("Destroy item");
                Destroy(m_Item);
                m_Item = null;
                m_CurrentActiveItem = null;
                m_ItemId = -1;
            }
            m_RemainTime = 0f;
            m_PlayerRuntimeData.UsingActiveItem = false;
            m_PlayerRuntimeData.AniParameter.UseActiveItem = false;
            m_PlayerRuntimeData.AniParameter.UseTime = 1f;
        }

        private void EndUse()
        {
            if(m_CurrentActiveItem == null) {
                return;
            }
            m_CurrentActiveItem.Use(gameObject);
            m_Inventory.ReduceItemAmount(m_ItemId);
            print("reduce " + m_ItemId);
            StopUse();
        }

        //[ServerRpc]
        //private void EndUseServerRpc() => EndUseClientRpc();

        [Rpc(SendTo.NotOwner)]
        private void OnUseActiveItemRpc(int itemId)
        {
            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(itemId);
            StartCoroutine(AssetCache.GetOrLoad(
                itemData.Prefab, obj => {
                    m_Item = obj;
                    m_CurrentActiveItem = m_Item.GetComponent<IActiveItem>();
                    m_Item.transform.SetParent(Hand, false);
                })
            );
        }

        [Rpc(SendTo.NotOwner)]
        private void EndUseRpc()
        {
            print("EndUseRpc");
            if(m_Item) {
                print("Destroy item");
                Destroy(m_Item);
                m_Item = null;
                m_CurrentActiveItem = null;
            }
        }

        #endregion
    }
}
