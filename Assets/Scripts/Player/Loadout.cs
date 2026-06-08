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
        // Only Use by Server
        NetworkVariable<NetworkObjectReference> m_WeaponRef1 = new NetworkVariable<NetworkObjectReference>();
        NetworkVariable<NetworkObjectReference> m_WeaponRef2 = new NetworkVariable<NetworkObjectReference>();

        private IWeapon m_WeaponSlot1 = null;
        private IWeapon m_WeaponSlot2 = null;

        public GameObject DefaultWeapon;
        public AttachableNode WeaponPlaceRoot;

        PlayerController m_Player = null;

        // Shield
        Shield m_Shield;

        // Action
        public Action<IWeapon, int> OnAddWeapon;
        public Action<IWeapon> OnRemoveWeapon;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_Player = GetComponentInParent<PlayerController>();
            if(IsOwner) {
                EventManager.AddListener<SwapWeaponEvent>(OnSwapWeapon);
            }
            if(IsServer) {
                m_WeaponRef1.OnValueChanged += OnWeaponRef1Changed;
                m_WeaponRef2.OnValueChanged += OnWeaponRef2Changed;
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
            return m_WeaponSlot1 == null || m_WeaponSlot2 == null;
        }

        public bool HasWeapon(int index)
        {
            if (index == 1) {
                return m_WeaponSlot1 != null;
            } else if (index == 2) {
                return m_WeaponSlot2 != null;
            }
            return false;
        }

        #region equip

        public int EquipWeapon(IWeapon weapon)
        {
            if (weapon == null) {
                Debug.LogError("err weapon");
            }
            if (EquipWeapon(weapon, m_WeaponRef1)) {
                OnAddWeapon?.Invoke(weapon, 1);
                return 1;
            } else if (EquipWeapon(weapon, m_WeaponRef2)) {
                OnAddWeapon?.Invoke(weapon, 2);
                return 2;
            }
            return -1;
        }

        bool EquipWeapon(IWeapon weapon, NetworkVariable<NetworkObjectReference> weaponSlot)
        {
            
            if (weaponSlot.Value.TryGet(out var go)) {
                return false;
            }

            weapon.Initialize(m_Player.gameObject);
            weaponSlot.Value = new NetworkObjectReference(weapon.GetNO());
            // Server端修改后，weaponSlot的ValueChanged能马上触发吗？

            
            return true;
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

            var instance = Instantiate(weaponPrefab);
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
            yield return new WaitUntil(() => no.IsSpawned);

            EquipWeapon(weapon);
        }

        public void UnequipWeapon(IWeapon weapon)
        {
            if (weapon == m_WeaponSlot1) {
                m_WeaponRef1 = default;
            } else if (weapon == m_WeaponSlot2) {
                m_WeaponRef2 = default;
            } else {
                return;
            }

            OnRemoveWeapon?.Invoke(weapon);
            EventManager.Broadcast(new UpdateLoadoutUIEvent {
                Weapon1 = ParseWeapon(m_WeaponSlot1),
                Weapon2 = ParseWeapon(m_WeaponSlot1)
            });
        }

        #endregion

        public bool ValidSlotIdx(int index)
        {
            return index == 1 || index == 2;
        }

        public IWeapon GetWeapon(int index)
        {
            if (!ValidSlotIdx(index)) {
                return null;
            }
            return m_WeaponSlot1;
        }

        public List<IWeapon> GetAllWeapon()
        {
            return new List<IWeapon> { m_WeaponSlot1, m_WeaponSlot2 };
        }

        [ServerRpc]
        void SwapWeaponServerRpc()
        {
            var temp = m_WeaponRef1;
            m_WeaponRef2 = m_WeaponRef1;
            m_WeaponRef1 = temp;
        }

        void OnSwapWeapon(SwapWeaponEvent evt)
        {
            SwapWeaponServerRpc();
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
                ParseWeapon(m_WeaponSlot1),
                ParseWeapon(m_WeaponSlot2)
            };

            return result;
        }
        #endregion

        private void OnWeaponRef1Changed(NetworkObjectReference pre, NetworkObjectReference cur)
        {
            if (cur.TryGet(out var networkObject)) {
                m_WeaponSlot1 = networkObject.GetComponentInChildren<IWeapon>();
                m_WeaponSlot1.Attach(WeaponPlaceRoot);
            } else {
                m_WeaponSlot1 = null;
            }
            EventManager.Broadcast(new UpdateLoadoutUIEvent {
                Weapon1 = ParseWeapon(m_WeaponSlot1),
                Weapon2 = ParseWeapon(m_WeaponSlot2)
            });
        }
        private void OnWeaponRef2Changed(NetworkObjectReference pre, NetworkObjectReference cur)
        {
            if (cur.TryGet(out var networkObject)) {
                m_WeaponSlot2 = networkObject.GetComponentInChildren<IWeapon>();
                m_WeaponSlot2.Attach(WeaponPlaceRoot);
            } else {
                m_WeaponSlot2 = null;
            }
            EventManager.Broadcast(new UpdateLoadoutUIEvent {
                Weapon1 = ParseWeapon(m_WeaponSlot1),
                Weapon2 = ParseWeapon(m_WeaponSlot2)
            });
        }
    }
}
