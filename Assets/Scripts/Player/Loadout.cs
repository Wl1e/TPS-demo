using System;
using System.Collections;
using System.Collections.Generic;
using TPSDemo.UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TPSDemo
{
    public class Loadout : NetworkBehaviour
    {
        // Only Use by Server
        private NetworkVariable<NetworkObjectReference> m_WeaponRef1 = new(default);

        private NetworkVariable<NetworkObjectReference> m_WeaponRef2 = new(default);

        private IWeapon m_WeaponSlot1 = null;
        private IWeapon m_WeaponSlot2 = null;


        public ItemData DefaultWeapon;
        public AttachableNode WeaponPlaceRoot;

        private PlayerController m_Player = null;

        // Shield
        private Shield m_Shield;

        // Action
        public Action<IWeapon, int> OnAddWeapon;

        public Action<IWeapon> OnRemoveWeapon;

        private void Awake()
        {
            m_Player = GetComponentInParent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_WeaponRef1.OnValueChanged += OnWeapon1Changed;
            m_WeaponRef2.OnValueChanged += OnWeapon2Changed;
            if (IsOwner) {
                EventManager.AddListener<Event.SwapWeaponEvent>(OnSwapWeapon);
                EventManager.AddListener<Event.TryUnequipWeaponEvent>(UnequipWeapon);
                if (DefaultWeapon != null) {
                    EquipWeapon(DefaultWeapon);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            m_WeaponRef1.OnValueChanged -= OnWeapon1Changed;
            m_WeaponRef2.OnValueChanged -= OnWeapon2Changed;
            if (IsOwner) {
                EventManager.RemoveListener<Event.SwapWeaponEvent>(OnSwapWeapon);
                EventManager.RemoveListener<Event.TryUnequipWeaponEvent>(UnequipWeapon);
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


        #region Client

        public void EquipWeapon(ItemData WeaponData) => EquipWeaponServerRpc(WeaponData.Id);

        public void UnequipWeapon(Event.TryUnequipWeaponEvent evt) => UnequipWeaponServerRpc(evt.WeaponIdx);

        #endregion


        #region equip(Server)

        /// <summary>
        /// 将武器放在空槽上
        /// </summary>
        /// <returns>
        /// 放置槽位，如果没有位置，返回-1
        /// </returns>
        public int EquipWeapon(NetworkObject weaponNO)
        {
            print("TryEquipWeapon");
            var weapon = weaponNO.GetComponentInChildren<IWeapon>();
            if (weapon == null) {
                Debug.LogError("err weapon");
            }
            if (!m_WeaponRef1.Value.TryGet(out var _)) {
                print($"设置m_WeaponRef1: {m_WeaponRef1.Value.NetworkObjectId} to {weaponNO.NetworkObjectId}");
                m_WeaponRef1.Value = weaponNO;
                return 1;
            } else if (!m_WeaponRef2.Value.TryGet(out var _)) {
                print($"设置m_WeaponRef2: {m_WeaponRef2.Value.NetworkObjectId} to {weaponNO.NetworkObjectId}");
                m_WeaponRef2.Value = weaponNO;
                return 2;
            }
            return -1;
        }

        [ServerRpc]
        private void WeaponAttachServerRpc(int idx)
        {
            if (idx == 1) {
                m_WeaponSlot1.Attach(WeaponPlaceRoot);
            } else if (idx == 2) {
                m_WeaponSlot2.Attach(WeaponPlaceRoot);
            }
        }

        [ServerRpc]
        private void EquipWeaponServerRpc(int weaponId)
        {
            if (!CanAddWeapon()) {
                print($"Cant Add Weapon weapon1: {m_WeaponSlot1}, weapon2: {m_WeaponSlot2}");
                return;
            }
            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(weaponId);
            StartCoroutine(EquipWeaponCoroutine(itemData));
        }

        /// <summary>
        /// 通过ItemData创建武器，并检查合法性，然后装备武器
        /// </summary>
        /// <param name="itemData"> 武器ItemData </param>
        private IEnumerator EquipWeaponCoroutine(ItemData itemData)
        {
            yield return null;

            NetworkObject instance = null;
            yield return WorldItemManager.Instance.CreateItemGO<NetworkObject>(
                itemData,
                Vector3.zero,
                Quaternion.identity,
                null,
                obj => {
                    if(!obj) {
                        return;
                    }
                    instance = obj;

                    var attachable = instance.GetComponentInChildren<AttachableBehaviour>();
                    if (attachable == null) {
                        Destroy(instance);
                        return;
                    }

                    if (!attachable.TryGetComponent<IWeapon>(out var weapon)) {
                        Destroy(instance);
                        return;
                    }
                    EquipWeapon(instance);
                },
                OwnerClientId
            );
        }

        [ServerRpc]
        public void UnequipWeaponServerRpc(int weaponIdx)
        {
            var weapon = GetWeapon(weaponIdx);
            if(weapon == null) {
                return;
            }

            if (weaponIdx == 1) {
                print($"清空m_WeaponRef1: {m_WeaponRef1.Value.NetworkObjectId} to default");
                m_WeaponRef1.Value = default;
            } else if (weaponIdx == 2) {
                print($"清空m_WeaponRef2: {m_WeaponRef2.Value.NetworkObjectId} to default");
                m_WeaponRef2.Value = default;
            } else {
                return;
            }

            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(weapon.WeaponId);
            StartCoroutine(WorldItemManager.Instance.SpawnItem(itemData, transform.position, 1));
        }

        [ServerRpc]
        private void SwapWeaponServerRpc()
        {
            var temp = m_WeaponRef1;
            m_WeaponRef2 = m_WeaponRef1;
            m_WeaponRef1 = temp;
        }

        #endregion equip

        public bool ValidSlotIdx(int index)
        {
            return index == 1 || index == 2;
        }

        public IWeapon GetWeapon(int index)
        {
            if (index == 1) {
                return m_WeaponSlot1;
            } else if (index == 2) {
                return m_WeaponSlot2;
            }
            return null;
        }

        public List<IWeapon> GetAllWeapon()
        {
            return new List<IWeapon> { m_WeaponSlot1, m_WeaponSlot2 };
        }

        private void OnSwapWeapon(Event.SwapWeaponEvent evt)
        {
            SwapWeaponServerRpc();
        }

        #region For UI

        private UI.WeaponUIData ParseWeapon(IWeapon weapon)
        {
            var data = new UI.WeaponUIData();
            if (weapon == null) {
                data.WeaopnId = 0;
                return data;
            }
            data.WeaopnId = weapon.WeaponId;
            data.CurAmmo = weapon.CurrentAmmo;
            data.MaxAmmo = m_Player.Inventory.GetAmount(weapon.AmmoId);
            data.AttachmentIdList = weapon.GetAttachmentList();
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

        #endregion For UI

        private void OnWeapon1Changed(NetworkObjectReference pre, NetworkObjectReference cur) => OnWeaponChanged(pre, cur, 1);

        private void OnWeapon2Changed(NetworkObjectReference pre, NetworkObjectReference cur) => OnWeaponChanged(pre, cur, 2);

        /// <summary>
        /// 会直接销毁旧武器，如果要交换武器，需新加方法
        /// </summary>
        private void OnWeaponChanged(NetworkObjectReference pre, NetworkObjectReference cur, int idx)
        {
            print("OnWeaponChanged");
            IWeapon weapon = GetWeapon(idx);

            // 删除旧武器
            if (weapon != null) {
                if (IsServer) {
                    weapon.Detach();
                    pre.TryGet(out var no);
                    print("Server销毁武器 " + idx);
                    if (no != null) {
                        no.Despawn();
                    }
                }
                if(IsOwner) {
                    print("Client卸下武器 " + idx);
                    weapon.OnAttachmentChanged -= OnWeaponAttachmentChanged;
                    OnRemoveWeapon?.Invoke(weapon);
                    //if (idx == 1) {
                    //    m_WeaponSlot1 = null;
                    //} else if (idx == 2) {
                    //    m_WeaponSlot2 = null;
                    //}
                }
            }

            weapon = null;
            // 初始化武器
            if (cur.TryGet(out var networkObject)) {
                    weapon = networkObject.GetComponentInChildren<IWeapon>();
                // Server需要weapon初始化，因为weapon的实际攻击逻辑在Server端
                if (IsServer || IsOwner) {
                    weapon.Initialize(m_Player.gameObject);
                }
                if (IsOwner) {
                    weapon.OnAttachmentChanged += OnWeaponAttachmentChanged;
                    OnAddWeapon?.Invoke(weapon, idx);
                }
            } else {
                print("没有新武器");
            }

            // 设置WeaponSlot
            if (idx == 1) {
                print($"设置新武器 {idx} 为 {weapon}");
                m_WeaponSlot1 = weapon;
            } else if (idx == 2) {
                print($"设置新武器 {idx} 为 {weapon}");
                m_WeaponSlot2 = weapon;
            }

            // 修改武器位置
            if (IsOwner && weapon != null) {
                // 为什么在这执行attach
                // 因为在Server执行EquipWeapon，设置WeaponRef后，
                // Client的OnWeaponChanged还未调用，如果那时直接Attach，
                // 会导致现在获取的NO下拿不到IWeapon（被Attach移到别的地方了）
                WeaponAttachServerRpc(idx);
            }

            // 更新UI
            if (IsOwner) {
                EventManager.Broadcast(new Event.UpdateLoadoutUIEvent {
                    Weapon1 = ParseWeapon(m_WeaponSlot1),
                    Weapon2 = ParseWeapon(m_WeaponSlot2)
                });
            }
        }

        private void OnWeaponAttachmentChanged()
        {
            if (IsOwner) {
                EventManager.Broadcast(new Event.UpdateLoadoutUIEvent {
                    Weapon1 = ParseWeapon(m_WeaponSlot1),
                    Weapon2 = ParseWeapon(m_WeaponSlot2)
                });
            }
        }
    }
}
