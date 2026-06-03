using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo.UI
{
    public struct WeaponUIData
    {
        public int WeaopnId;
        public int CurAmmo;
        public int MaxAmmo;
        // ItemData上没有存储配件的位置，需要传
        public List<(IAttachment.AttachmentSlot, int)> AttachmentIdList;
    }

    public class LoadoutUI : MonoBehaviour
    {
        public WeaponSlotUI WeaponSlot1;
        public WeaponSlotUI WeaponSlot2;

        bool m_IsOpened = false;

        private void Start()
        {
            EventManager.AddListener<Event.UpdateLoadoutUIEvent>(UpdateLoadout);
            // 装备槽在UI上是背包的一部分，为了降低Inventory复杂度，单独拿出来
            // 所以当背包打开/关闭时，装备槽也要跟着显示/隐藏
            EventManager.AddListener<Event.InventoryStateChangeEvent>(OnInventoryStateChange);
            gameObject.SetActive(m_IsOpened);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<Event.UpdateLoadoutUIEvent>(UpdateLoadout);
            EventManager.RemoveListener<Event.InventoryStateChangeEvent>(OnInventoryStateChange);
        }

        public void Initialize()
        {
            var data = PlayerDataProxy.Instance.GetLoadoutData();
            WeaponSlot1.SetWeapon(data[0]);
            WeaponSlot2.SetWeapon(data[1]);
        }

        public void SwapWeapon(int idx1, int idx2)
        {
            // 只有两个槽位，所以一定是1和2
            EventManager.Broadcast(new Event.SwapWeaponEvent { Idx1 = 1, Idx2 = 2 });
        }

        public void TryReload(int idx)
        {
            EventManager.Broadcast(new Event.TryReloadEvent { WeaponIdx = idx });
        }

        void UpdateLoadout(Event.UpdateLoadoutUIEvent evt)
        {
            WeaponSlot1.SetWeapon(evt.weapon1);
            WeaponSlot2.SetWeapon(evt.weapon2);
        }

        void OnInventoryStateChange(Event.InventoryStateChangeEvent evt)
        {
            m_IsOpened = evt.IsOpened;
            gameObject.SetActive(m_IsOpened);
        }
    }
}
