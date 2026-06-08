using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
using Event;
    using System.Collections;
    using Unity.Netcode.Components;

    public class Loadout: NetworkBehaviour
    {
        // Weapon
        NetworkList<NetworkObjectReference> m_WeaponRefs = new NetworkList<NetworkObjectReference>();
        List<IWeapon> m_WeaponSlots;
        [SerializeField] int m_MaxWeaponCount = 2;
        public GameObject DefaultWeapon;
        public int WeaponCount => m_WeaponSlots.FindAll(weapon => weapon != null).Count;
        public int MaxCount => m_MaxWeaponCount;
        public Transform WeaponPlaceRoot;

        PlayerController m_Player = null;

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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_Player = GetComponentInParent<PlayerController>();
            if(IsOwner) {
                EventManager.AddListener<SwapWeaponEvent>(OnSwapWeapon);
            }
            if(IsServer) {
                if (DefaultWeapon) {
                    EquipWeapon(DefaultWeapon);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) {
                EventManager.RemoveListener<SwapWeaponEvent>(OnSwapWeapon);
            }
            base.OnNetworkDespawn();
        }

        public bool CanAddWeapon()
        {
            return WeaponCount < m_MaxWeaponCount;
        }

        public bool HasWeapon(int index)
        {
            return m_WeaponSlots[index] != null;
        }

        #region equip

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
            if (weapon == null) {
                Debug.LogError("err weapon");
            }
            if (!ValidSlotIdx(index)) {
                return -1;
            }
            if (m_WeaponSlots[index] != null) {
                return -1;
            }

            weapon.Initialize(m_Player.gameObject);
            m_WeaponSlots[index] = weapon;
            OnAddWeapon?.Invoke(weapon, index);

            EventManager.Broadcast(new UpdateLoadoutUIEvent {
                Weapon1 = ParseWeapon(m_WeaponSlots[1]),
                Weapon2 = ParseWeapon(m_WeaponSlots[2])
            });
            return index;
        }

        public void EquipWeapon(GameObject WeaponPrefab)
        {
            StartCoroutine(EquipWeaponCoroutine(WeaponPrefab));
        }

        private IEnumerator EquipWeaponCoroutine(GameObject weaponPrefab)
        {
            yield return null;

            if(!CanAddWeapon()) {
                yield break;
            }

            var instance = Instantiate(weaponPrefab, WeaponPlaceRoot);
            if (!instance.TryGetComponent<NetworkObject>(out var no)) {
                Destroy(instance);
                yield break;
            }

            var attachable = instance.GetComponentInChildren<AttachableBehaviour>();
            if (attachable == null) {
                Destroy(instance);
                yield break;
            }

            if(!attachable.TryGetComponent<IWeapon>(out var weapon)) {
                Destroy(instance);
                yield break;
            }

            no.SpawnWithOwnership(OwnerClientId);
            m_WeaponRefs.Add(new NetworkObjectReference(instance));
            yield return new WaitUntil(() => no.IsSpawned);

            int idx = EquipWeapon(weapon);
            EquipWeaponClientRpc(weapon, idx);
        }

        [ClientRpc]
        void EquipWeaponClientRpc(IWeapon weapon, int idx)
        {
            EquipWeapon(weapon, idx);
        }

        public void UnequipWeapon(IWeapon weapon)
        {
            int idx = m_WeaponSlots.FindIndex(slot => slot == weapon);
            if (idx != -1) {
                m_WeaponSlots[idx] = null;
                OnRemoveWeapon?.Invoke(weapon);
                EventManager.Broadcast(new UpdateLoadoutUIEvent {
                    Weapon1 = ParseWeapon(m_WeaponSlots[1]),
                    Weapon2 = ParseWeapon(m_WeaponSlots[2])
                });
            }
        }

        #endregion

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
                    Weapon1 = ParseWeapon(m_WeaponSlots[1]),
                    Weapon2 = ParseWeapon(m_WeaponSlots[2])
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
            List<UI.WeaponUIData> result = new List<UI.WeaponUIData> {
                ParseWeapon(m_WeaponSlots[1]),
                ParseWeapon(m_WeaponSlots[2])
            };

            return result;
        }
        #endregion

        private void OnWeaponRefChanged(NetworkListEvent<NetworkObjectReference> refer)
        {
            if(refer.Index)
        }
    }
}
