using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    using Event;

    public class WeaponManager : FirearmCombatSlot
    {
        // IFirearmController
        private bool m_IsActive = false;

        public override bool IsActive => m_IsActive;
        public override float ReloadTime => m_ReloadTime;

        public int CurrentFirearmIndex => m_CurrentFirearmIndex.Value;
        public IWeapon CurrentFirearm => m_CurrentFirearm;

        public Action<int, bool> m_OnAttack;

        private PlayerRuntimeData m_RuntimeData;
        private Inventory m_Inventory;
        private Loadout m_Loadout;
        private NetworkVariable<int> m_CurrentFirearmIndex = new(-1);
        private IWeapon m_CurrentFirearm = null;
        [SerializeField] private float m_ReloadTime = 1f;

        private NetworkVariable<bool> m_Reloading = new(false);
        private Coroutine m_ReloadCoroutine = null;

        /// <summary>
        /// 瞄准对象
        /// </summary>
        public Transform AimTarget;

        /// <summary>
        /// 右手绑定位置
        /// </summary>
        public AttachableNode RightHandAttach;

        /// <summary>
        /// 背部绑定位置
        /// </summary>
        public AttachableNode BackAttach;

        private void Awake()
        {
            var playerController = GetComponentInParent<PlayerController>();
            m_Inventory = playerController.Inventory;
            m_RuntimeData = playerController.RuntimeData;
            m_Loadout = playerController.Loadout;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner) {
                m_Reloading.OnValueChanged += OnReloadStateChanged;
                m_Loadout.OnAddWeapon += OnWeaponAdded;
                m_Loadout.OnRemoveWeapon += OnWeaponRemoved;
                EventManager.AddListener<TryReloadEvent>(TryReload2);
                EventManager.AddListener<TryEquipAttachmentEvent>(FirearmTryEquipAttachment);

                var firearms = m_Loadout.GetAllWeapon();
                for (int i = 0; i < firearms.Count; ++i) {
                    if (firearms[i] != null) {
                        OnWeaponAdded(firearms[i], i);
                    }
                }
            }
            m_CurrentFirearmIndex.OnValueChanged += OnWeaponChanged;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) {
                m_Loadout.OnAddWeapon -= OnWeaponAdded;
                m_Loadout.OnRemoveWeapon -= OnWeaponRemoved;
                EventManager.RemoveListener<TryReloadEvent>(TryReload2);
                EventManager.RemoveListener<TryEquipAttachmentEvent>(FirearmTryEquipAttachment);
            }
            m_CurrentFirearmIndex.OnValueChanged -= OnWeaponChanged;
            base.OnNetworkDespawn();
        }

        /// <summary>
        /// 当武器被添加到Loadout时，添加武器射击回调
        /// </summary>
        /// <param name="weapon"> 射击的武器 </param>
        /// <param name="idx"> 射击武器的下标 </param>
        private void OnWeaponAdded(IWeapon weapon, int idx)
        {
            weapon.OnFire += OnWeaponFire;
        }

        /// <summary>
        /// 将武器射击的本地Action进行广播
        /// 其实可以让武器自己广播
        /// </summary>
        /// <param name="idx"></param>
        private void OnWeaponFire()
        {
            EventManager.Broadcast(new WeaponFiredEvent());
        }

        private void OnWeaponRemoved(IWeapon weapon)
        {
            weapon.OnFire -= OnWeaponFire;
            if(weapon == m_CurrentFirearm) {
                if (IsServer) {
                    m_CurrentFirearmIndex.Value = -1;
                }
            }
        }

        public override bool ValidActive() {
            print($"Weapon1 {m_Loadout.HasWeapon(1)}, Weapon2 {m_Loadout.HasWeapon(2)}");
            return m_Loadout != null && (m_Loadout.HasWeapon(1) || m_Loadout.HasWeapon(2));
        }

        public override void SetActive(bool isActive)
        {
            m_IsActive = isActive;
            if (!m_IsActive) {
                if (CurrentFirearmIndex != -1) {
                    m_CurrentFirearmIndex.Value = -1;
                }
                if (m_ReloadCoroutine != null) {
                    StopCoroutine(m_ReloadCoroutine);
                    m_ReloadCoroutine = null;
                    m_Reloading.Value = false;
                }
            }
        }

        public override bool ValidAim()
        {
            return m_CurrentFirearm != null;
        }

        public override void OnAim(bool isAiming)
        {
        }

        /// <summary>
        /// 武器装备和卸下时操作
        /// 1. 更改武器位置
        /// 2. 如果卸下武器，停止武器攻击、清空m_CurrentFirearmIndex和m_CurrentFirearm
        /// </summary>
        /// <param name="isEquip"> 为ture则装备；反之为卸下 </param>
        public override void Attack(bool down)
        {
            if (!m_RuntimeData.IsAiming) {
                return;
            }
            if (down) {
                m_CurrentFirearm?.StartFire(AimTarget);
                m_RuntimeData.AniParameter.Attack = down;
                RaiseAttack(0, true);
            } else {
                m_CurrentFirearm?.EndFire();
                m_RuntimeData.AniParameter.Attack = down;
                RaiseAttack(0, false);
            }
        }

        #region Equip

        [ServerRpc]
        private void TrySwitchFirearmServerRpc(int idx)
        {
            // 按下当前武器对应数字键收回武器
            if (CurrentFirearmIndex == idx) {
                m_CurrentFirearmIndex.Value = -1;
                return;
            }

            // 如果对应武器槽没有武器
            var newCurrentFirearm = m_Loadout.GetWeapon(idx);
            if (newCurrentFirearm == null) {
                return;
            }

            m_CurrentFirearmIndex.Value = idx;
        }

        public override void TrySwitchFirearm(int idx)
        {
            if (!m_IsActive) {
                return;
            }
            TrySwitchFirearmServerRpc(idx);
        }

        #endregion Equip

        #region Reload

        [ServerRpc]
        private void TryReloadServerRpc()
        {
            if (!ValidReload()) {
                return;
            }
            int amount = m_CurrentFirearm.CurrentAmmo;
            int ammoId = m_CurrentFirearm.AmmoId;
            m_CurrentFirearm.StartReload();
            PutAmmoIntoInventory(ammoId, amount);
            m_ReloadCoroutine = StartCoroutine(ReloadCoroutine(m_CurrentFirearm.ReloadTime));
            //StartReloadClientRpc();
        }

        private bool ValidReload()
        {
            if(m_Reloading.Value) {
                print("正在换弹中");
                return false;
            }
            if (CurrentFirearmIndex == -1) {
                print("当前未装备武器");
                return false;
            }
            if (!m_CurrentFirearm.ValidReload() || m_Inventory.GetAmount(m_CurrentFirearm.AmmoId) <= 0) {
                print("武器无需换弹或没有对应子弹");
                return false;
            }
            return true;
        }

        public override void TryReload()
        {
            if(!ValidReload()) {
                return;
            }
            TryReloadServerRpc();
        }

        // 当通过在UI中拖动子弹到武器槽时执行
        private void TryReload2(TryReloadEvent evt)
        {
            // FIXME: 界面拖动是有可能让当前未持有的武器换弹的，怎么办，要切枪吗
            TryReload();
        }

        private void PutAmmoIntoInventory(int ammoId, int amount)
        {
            if (amount > 0) {
                m_Inventory.AddItem(ammoId, amount);
            }
        }

        private int GetLoadAmmo(IWeapon weapon)
        {
            int ammoId = weapon.AmmoId;
            int ammoAmount = m_Inventory.GetAmount(ammoId);
            ammoAmount = Mathf.Min(ammoAmount, weapon.ClipAmmo);
            ammoAmount = m_Inventory.ReduceItemAmount(ammoId, ammoAmount);
            return ammoAmount;
        }

        private IEnumerator ReloadCoroutine(float time)
        {
            m_Reloading.Value = true;
            yield return new WaitForSeconds(time);
            m_CurrentFirearm.EndReload(GetLoadAmmo(m_CurrentFirearm));

            m_Reloading.Value = false;
            m_ReloadCoroutine = null;
            //EndReloadClientRpc();
        }

        private void OnReloadStateChanged(bool previousValue, bool newValue)
        {
            if(!IsOwner) {
                return;
            }
            if(newValue) {
                EventManager.Broadcast(new WeaponStartReloadEvent {
                    WeaponIdx = CurrentFirearmIndex
                });
            } else {
                EventManager.Broadcast(new WeaponEndReloadEvent {
                    WeaponIdx = CurrentFirearmIndex
                });
            }
            m_RuntimeData.AniParameter.Reload = newValue;
        }

        #endregion Reload

        // 非Owner检测到武器更换时同步更换
        private void OnWeaponChanged(int pre, int cur)
        {
            // m_ReloadCoroutine修改起来太麻烦了，后续通过WeaponStateManager同步
            if (IsServer) {
                // m_CurrentFirearm当是卸下weapon时，不需要触发，不然将武器背到背上
                if (m_CurrentFirearm != null) {
                    m_CurrentFirearm?.Attach(BackAttach);
                }

                if (m_ReloadCoroutine != null) {
                    StopCoroutine(m_ReloadCoroutine);
                    m_ReloadCoroutine = null;
                    m_Reloading.Value = false;
                }
            }

            if (IsServer || IsOwner) {
                if (CurrentFirearmIndex == -1) {
                    if (IsOwner) {
                        m_CurrentFirearm.EndFire();
                        m_CurrentFirearm.OnUnequip();
                        Exit();
                    }
                    m_CurrentFirearm = null;
                } else {
                    m_CurrentFirearm = m_Loadout.GetWeapon(cur);
                    if (IsServer) {
                        if (m_CurrentFirearm != null) {
                            m_CurrentFirearm.Attach(RightHandAttach);
                            m_CurrentFirearm.OnEquip();
                        }
                    }
                    //if (IsOwner) {
                    //}
                }
            }

            if (IsOwner) {
                EventManager.Broadcast(new WeaponChangedEvent { OldIdx = pre, NewIdx = CurrentFirearmIndex });
            }
        }

        private void FirearmTryEquipAttachment(TryEquipAttachmentEvent evt)
        {
            var weapon = m_Loadout.GetWeapon(evt.WeaponIdx);
            if (weapon == null) {
                return;
            }

            var item = m_Inventory.GetItem(evt.InventoryIdx);
            //if (!item.ItemData.Prefab.TryGetComponent<IAttachment>(out var attachment)) {
            //    return;
            //}
            var attachmentData = item.ItemData as AttachmentItemData;

            if (!weapon.SupportAttachment(attachmentData.Slot, attachmentData.Id)) {
                return;
            }

            weapon.AddAttachment(attachmentData.Slot, attachmentData.Id);
            m_Inventory.RemoveItem(evt.InventoryIdx);
        }
    }
}
