using UnityEngine;

namespace TPSDemo
{

    public class ItemUsageController : MonoBehaviour
    {
        private GameObject m_Item = null;
        private IActiveItem m_CurrentActiveItem = null;
        private int m_ItemId = -1;
        private float m_RemainTime = 0f;
        private bool m_Using = false;
        private Inventory m_Inventory;
        private PlayerRuntimeData m_PlayerRuntimeData;

        [SerializeField] private Transform RightHand;

        private void Awake()
        {
            // 访问背包是一个很常见的需求，可以考虑再黑板或Player上加个方法
            var player = GetComponent<PlayerController>();
            m_Inventory = player.Inventory;
            m_PlayerRuntimeData = player.RuntimeData;
        }

        private void OnEnable()
        {
            EventManager.AddListener<Event.TryUseActiveItemEvent>(UseActiveItem);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Event.TryUseActiveItemEvent>(UseActiveItem);
        }

        private void UseActiveItem(Event.TryUseActiveItemEvent evt)
        {
            var itemData = m_Inventory.GetItem(evt.InventorySlotId).ItemData;
            m_ItemId = itemData.Id;
            StartCoroutine(WorldItemManager.Instance.CreateItemGO(itemData, obj => {
                m_Item = obj;
                m_Item.transform.SetParent(RightHand, false);
                StartUse(obj.GetComponent<IActiveItem>());
            }));
        }

        private void Update()
        {
            if (!m_Using || m_CurrentActiveItem == null) {
                return;
            }

            if (m_RemainTime > 0f) {
                m_RemainTime -= Time.deltaTime;
            } else {
                EndUse();
            }
        }

        public void StartUse(IActiveItem item)
        {
            print("StartUse");
            m_CurrentActiveItem = item;
            m_RemainTime = item.UseTime;
            m_Using = true;
            m_PlayerRuntimeData.UsingActiveItem = true;
            m_PlayerRuntimeData.AniParameter.UseActiveItem = true;
            m_PlayerRuntimeData.AniParameter.UseTime = item.UseTime;
        }

        public void StopUse()
        {
            if(m_Using) {
                ResetVariables();
            }
        }

        private void ResetVariables()
        {
            m_Using = false;
            if(m_Item != null) {
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
            print("EndUse");
            if(m_CurrentActiveItem == null) {
                return;
            }
            m_CurrentActiveItem.Use(gameObject);
            m_Inventory.ReduceItemAmount(m_ItemId);
            ResetVariables();
        }
    }
}
