using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
using Event;
    public class Loadout : MonoBehaviour
    {
        // Weapon
        List<IWeapon> m_WeaponSlots;
        [SerializeField] int m_MaxWeaponCount = 2;
        public GameObject DefaultWeapon;
        public int WeaponCount => m_WeaponSlots.FindAll(weapon => weapon != null).Count;
        public int MaxCount => m_MaxWeaponCount;
        public Transform WeaponPlaceRoot;

        PlayerController m_Player;

        // Shield
        Shield m_Shield;

        // Action
        public Action<IWeapon, int> OnAddWeapon;
        public Action<IWeapon> OnRemoveWeapon;

        private void Awake()
        {
            m_WeaponSlots = new List<IWeapon> {
            null,
            null,
            null
        };
        }

        private void OnEnable()
        {
            EventManager.AddListener<SwapWeaponEvent>(OnSwapWeapon);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<SwapWeaponEvent>(OnSwapWeapon);
        }

        private void Start()
        {
            m_Player = GetComponentInParent<PlayerController>();
            if (DefaultWeapon) {
                EquipWeapon(DefaultWeapon);
            }
        }

        public bool CanAddWeapon()
        {
            return WeaponCount < m_MaxWeaponCount;
        }

        public bool HasWeapon(int index)
        {
            return m_WeaponSlots[index] != null;
        }

        public int EquipWeapon(IWeapon weapon)
        {
            if (EquipWeapon(weapon, 1) > 0) {
                return 1;
            } else if (EquipWeapon(weapon, 2) > 0) {
                return 2;
            }
            return -1;
        }

        int EquipWeapon(IWeapon weapon, int index)
        {
            if (!ValidSlotIdx(index)) {
                return -1;
            }
            if (m_WeaponSlots[index] != null) {
                return -1;
            }
            weapon.Initialize(m_Player.gameObject);
            m_WeaponSlots[index] = weapon;
            OnAddWeapon?.Invoke(weapon, index);

            var evt = new UpdateLoadoutUIEvent();

            var data = GetUIData();
            evt.weapon1 = data[0];
            evt.weapon2 = data[1];

            EventManager.Broadcast(evt);
            return index;
        }

        public int EquipWeapon(GameObject WeaponPrefab)
        {
            var instance = Instantiate(WeaponPrefab, WeaponPlaceRoot);
            return EquipWeapon(instance.GetComponent<IWeapon>());
        }

        public void UnequipWeapon(IWeapon weapon)
        {
            int idx = m_WeaponSlots.FindIndex(slot => slot == weapon);
            if (idx != -1) {
                m_WeaponSlots[idx] = null;
                OnRemoveWeapon?.Invoke(weapon);
            }
        }

        public bool ValidSlotIdx(int index)
        {
            return index > 0 && index <= m_MaxWeaponCount;
        }

        public IWeapon GetWeapon(int index)
        {
            if (!ValidSlotIdx(index)) {
                return null;
            }
            return m_WeaponSlots[index];
        }

        public List<IWeapon> GetAllWeapon()
        {
            return m_WeaponSlots;
        }

        void OnSwapWeapon(SwapWeaponEvent evt)
        {
            if (!ValidSlotIdx(evt.Idx1) || !ValidSlotIdx(evt.Idx2)) {
                return;
            }
            var temp = m_WeaponSlots[evt.Idx1];
            m_WeaponSlots[evt.Idx1] = m_WeaponSlots[evt.Idx2];
            m_WeaponSlots[evt.Idx2] = temp;
            EventManager.Broadcast(
                new UpdateLoadoutUIEvent {
                    weapon1 = ParseWeapon(m_WeaponSlots[1]),
                    weapon2 = ParseWeapon(m_WeaponSlots[2])
                }
            );
        }

        #region For UI
        UI.WeaponUIData ParseWeapon(IWeapon weapon)
        {
            var data = new UI.WeaponUIData();
            if (weapon == null) {
                data.WeaopnId = 0;
                return data;
            }
            data.WeaopnId = weapon.WeaponId;
            data.CurAmmo = weapon.CurrentAmmo;
            data.MaxAmmo = m_Player.Inventory.GetAmount(weapon.AmmoId);
            data.AttachmentIdList = new List<(IAttachment.AttachmentSlot, int)>();
            foreach (var attachment in weapon.Attachments) {
                data.AttachmentIdList.Add((attachment.Key, attachment.Value.Id));
            }
            return data;
        }

        public List<UI.WeaponUIData> GetUIData()
        {
            List<UI.WeaponUIData> result = new List<UI.WeaponUIData>();
            result.Add(ParseWeapon(m_WeaponSlots[1]));
            result.Add(ParseWeapon(m_WeaponSlots[2]));

            return result;
        }
        #endregion
    }
}
