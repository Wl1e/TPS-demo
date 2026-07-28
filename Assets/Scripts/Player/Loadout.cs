using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace TPSDemo
{
    public class Loadout : NetworkBehaviour
    {
        // Only Use by Server
        private NetworkVariable<NetworkObjectReference> m_WeaponRef1 = new((NetworkObject)null);

        private NetworkVariable<NetworkObjectReference> m_WeaponRef2 = new((NetworkObject)null);

        private IWeapon m_WeaponSlot1 = null;
        private IWeapon m_WeaponSlot2 = null;


        public WeaponItemData DefaultWeapon;
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

        private void SetWeapon(int idx, IWeapon weapon)
        {
            if (idx == 1) {
                Debug.Log($"设置新武器 {idx} 为 {weapon}");
                m_WeaponSlot1 = weapon;
            } else if (idx == 2) {
                Debug.Log($"设置新武器 {idx} 为 {weapon}");
                m_WeaponSlot2 = weapon;
            }

            if (IsOwner) {
                // 更新UI
                EventManager.Broadcast(new Event.UpdateLoadoutUIEvent {
                    Weapon1 = ParseWeapon(m_WeaponSlot1),
                    Weapon2 = ParseWeapon(m_WeaponSlot2)
                });
            }
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

        

        public void UnequipWeapon(Event.TryUnequipWeaponEvent evt) => UnequipWeaponServerRpc(evt.WeaponIdx);

        #endregion


        #region equip(Server)

        public IEnumerator EquipWeaponCo(WeaponItemData weaponData, int ammo)
        {
            if (!CanAddWeapon()) {
                yield break;
            }
            yield return EquipWeaponCoroutine(weaponData, ammo);
        }

        public void EquipWeapon(ItemPickup weaponPickup)
        {
            StartCoroutine(EquipWeaponCo((weaponPickup.Data as WeaponItemData), weaponPickup.Amount));
        }

        /// <summary>
        /// 将武器放在空槽上
        /// </summary>
        /// <returns>
        /// 放置槽位，如果没有位置，返回-1
        /// </returns>
        public int EquipWeapon(NetworkObject weaponNO)
        {
            var weapon = weaponNO.GetComponentInChildren<IWeapon>();
            if (weapon == null) {
                Debug.LogError("err weapon");
            }
            if (!m_WeaponRef1.Value.TryGet(out var _)) {
                Debug.Log($"clientId: {m_Player.Id}, 设置m_WeaponRef1: {m_WeaponRef1.Value} to {weaponNO}");
                m_WeaponRef1.Value = weaponNO;
                return 1;
            } else if (!m_WeaponRef2.Value.TryGet(out var _)) {
                Debug.Log($"clientId: {m_Player.Id}, 设置m_WeaponRef2: {m_WeaponRef2.Value} to {weaponNO}");
                m_WeaponRef2.Value = weaponNO;
                return 2;
            }
            return -1;
        }

        private void WeaponAttachToPlaceRoot(int idx)
        {
            if (idx == 1) {
                m_WeaponSlot1.Attach(WeaponPlaceRoot);
            } else if (idx == 2) {
                m_WeaponSlot2.Attach(WeaponPlaceRoot);
            }
        }

        [ServerRpc]
        private void WeaponAttachServerRpc(int idx) => WeaponAttachToPlaceRoot(idx);

        /// <summary>
        /// 通过ItemData创建武器，并检查合法性，然后装备武器
        /// </summary>
        /// <param name="itemData"> 武器ItemData </param>
        private IEnumerator EquipWeaponCoroutine(WeaponItemData itemData, int ammo)
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
                        Debug.LogError("Weapon spawn fail");
                        return;
                    }
                    instance = obj;

                    var attachable = instance.GetComponentInChildren<AttachableBehaviour>();
                    if (attachable == null) {
                        Debug.LogError("no attachable");
                        Destroy(instance);
                        return;
                    }

                    if (!attachable.TryGetComponent<IWeapon>(out var weapon)) {
                        Debug.LogError("no iweapon");
                        Destroy(instance);
                        return;
                    }
                    obj.DestroyWithScene = false;
                    weapon.SetAmmo(ammo);
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

            //var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(weapon.WeaponId);
            StartCoroutine(WorldItemManager.Instance.SpawnItem(
                weapon.Config, transform.position, weapon.CurrentAmmo, null, true)
            );

            if (weaponIdx == 1) {
                m_WeaponRef1.Value = (GameObject)null;
            } else if (weaponIdx == 2) {
                m_WeaponRef2.Value = (GameObject)null;
            }
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
                data.WeaponId = 0;
                return data;
            }
            data.WeaponId = weapon.WeaponId;
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

        private void OnWeapon1Changed(NetworkObjectReference pre, NetworkObjectReference cur)
        {
            OnWeaponChanged(pre, cur, 1);
        }

        private void OnWeapon2Changed(NetworkObjectReference pre, NetworkObjectReference cur) => OnWeaponChanged(pre, cur, 2);

        /// <summary>
        /// 会直接销毁旧武器，如果要交换武器，需新加方法
        /// </summary>
        private void OnWeaponChanged(NetworkObjectReference pre, NetworkObjectReference cur, int idx)
        {
            IWeapon weapon = GetWeapon(idx);

            // 删除旧武器
            if (weapon != null) {
                if(IsOwner) {
                    Debug.Log("Client卸下武器 " + idx);
                    weapon.OnAttachmentChanged -= OnWeaponAttachmentChanged;
                    OnRemoveWeapon?.Invoke(weapon);
                    //if (idx == 1) {
                    //    m_WeaponSlot1 = null;
                    //} else if (idx == 2) {
                    //    m_WeaponSlot2 = null;
                    //}
                }
                if (IsServer) {
                    weapon.Detach();
                    pre.TryGet(out var no);
                    Debug.Log("Server销毁武器 " + idx);
                    if (no != null) {
                        no.Despawn();
                    }
                }
            }

            bool unequip = (cur.NetworkObjectId == ulong.MaxValue);

            if (IsServer) {
                weapon = null;
                if (cur.TryGet(out var no)) {
                    weapon = no.GetComponentInChildren<IWeapon>();
                    // Server需要weapon初始化，因为weapon的实际攻击逻辑在Server端
                    weapon.Initialize(m_Player.gameObject);
                }
                // 设置WeaponSlot
                SetWeapon(idx, weapon);

                //if (IsHost && weapon != null) {
                //    weapon.OnAttachmentChanged += OnWeaponAttachmentChanged;
                //    OnAddWeapon?.Invoke(weapon, idx);
                //}
            }
            if (IsOwner) {
                if (unequip) {
                    SetWeapon(idx, null);
                }
                StartCoroutine(WaitWeaponSpawn(idx, cur.NetworkObjectId));
            }

            //weapon = null;
            //// 初始化武器
            //if (cur.TryGet(out var networkObject)) {
            //    weapon = networkObject.GetComponentInChildren<IWeapon>();
            //    // Server需要weapon初始化，因为weapon的实际攻击逻辑在Server端
            //    if (IsServer || IsOwner) {
            //        weapon.Initialize(m_Player.gameObject);
            //    }
            //    if (IsOwner) {
            //        weapon.OnAttachmentChanged += OnWeaponAttachmentChanged;
            //        OnAddWeapon?.Invoke(weapon, idx);
            //    }
            //} else {
            //    Log.Debug("没有新武器");
            //}

            //// 设置WeaponSlot
            //if (idx == 1) {
            //    Log.Debug($"设置新武器 {idx} 为 {weapon}");
            //    m_WeaponSlot1 = weapon;
            //} else if (idx == 2) {
            //    Log.Debug($"设置新武器 {idx} 为 {weapon}");
            //    m_WeaponSlot2 = weapon;
            //}

            //// 修改武器位置
            //if (IsOwner && weapon != null) {
            //    // 为什么在这执行attach
            //    // 因为在Server执行EquipWeapon，设置WeaponRef后，
            //    // Client的OnWeaponChanged还未调用，如果那时直接Attach，
            //    // 会导致现在获取的NO下拿不到IWeapon（被Attach移到别的地方了）
            //    WeaponAttachToPlaceRoot(idx);
            //}

            //// 更新UI
            //if (IsOwner) {
            //    EventManager.Broadcast(new Event.UpdateLoadoutUIEvent {
            //        Weapon1 = ParseWeapon(m_WeaponSlot1),
            //        Weapon2 = ParseWeapon(m_WeaponSlot2)
            //    });
            //}
        }

        // Server生成武器后，武器还没在Client生成，需要等待Client生成武器后继续逻辑
        private IEnumerator WaitWeaponSpawn(int idx, ulong id)
        {
            if (!IsOwner) {
                yield break;
            }

            NetworkObject obj = null;
            while (obj == null) {
                GameNetworkManager.Instance.SpawnManager.SpawnedObjects.TryGetValue(id, out obj);
                yield return null;
            }

            IWeapon weapon = obj.GetComponentInChildren<IWeapon>();
            // 初始化武器
            weapon.Initialize(m_Player.gameObject);
            weapon.OnAttachmentChanged += OnWeaponAttachmentChanged;
            OnAddWeapon?.Invoke(weapon, idx);

            // 设置WeaponSlot
            SetWeapon(idx, weapon);

            // 修改武器位置
            if (weapon != null) {
                // 为什么在这执行attach
                // 因为在Server执行EquipWeapon，设置WeaponRef后，
                // Client的OnWeaponChanged还未调用，如果那时直接Attach，
                // 会导致现在获取的NO下拿不到IWeapon（被Attach移到别的地方了）
                WeaponAttachServerRpc(idx);
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
