using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo.UI
{
    public struct WeaponUIData
    {
        public int WeaponId;
        public int CurAmmo;
        public int MaxAmmo;
        // ItemData上没有存储配件的位置，需要传
        public List<(IAttachment.AttachmentSlot, int)> AttachmentIdList;
    }

    public class LoadoutUI : MonoBehaviour, IPanel
    {
        public WeaponSlotUI WeaponSlot1;
        public WeaponSlotUI WeaponSlot2;

        bool m_IsOpened = false;

        private void Start()
        {
            EventManager.AddListener<Event.UpdateLoadoutUIEvent>(UpdateLoadout);
            gameObject.SetActive(m_IsOpened);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<Event.UpdateLoadoutUIEvent>(UpdateLoadout);
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
            WeaponSlot1.SetWeapon(evt.Weapon1);
            WeaponSlot2.SetWeapon(evt.Weapon2);
        }

        public void Open()
        {
            m_IsOpened = true;
            gameObject.SetActive(m_IsOpened);
        }

        public void Close()
        {
            m_IsOpened = false;
            gameObject.SetActive(m_IsOpened);
        }
    }
}
