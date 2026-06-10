using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
using Event;
    using System.Runtime.ConstrainedExecution;

    public class WeaponManager : FirearmCombatSlot
    {
        // IFirearmController
        bool m_IsActive = false;
        public override bool IsActive => m_IsActive;
        public override float ReloadTime => m_ReloadTime;

        public int CurrentFirearmIndex => m_CurrentFirearmIndex.Value;
        public IWeapon CurrentFirearm => m_CurrentFirearm;

        public Action<int, bool> m_OnAttack;


        PlayerRuntimeData m_RuntimeData;
        Inventory m_Inventory;
        Loadout m_Loadout;
        NetworkVariable<int> m_CurrentFirearmIndex = new NetworkVariable<int>(-1);
        IWeapon m_CurrentFirearm = null;
        [SerializeField] float m_ReloadTime = 1f;

        private NetworkVariable<bool> m_Reloading = new NetworkVariable<bool>(false);
        Coroutine m_ReloadCoroutine = null;

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
                m_Loadout.OnAddWeapon += OnWeaponAdded;
                m_Loadout.OnRemoveWeapon += OnWeaponRemoved;
                EventManager.AddListener<TryReloadEvent>(TryReload2);

                var firearms = m_Loadout.GetAllWeapon();
                int index = -1;
                for (int i = 0; i < firearms.Count; ++i) {
                    if (firearms[i] != null) {
                        OnWeaponAdded(firearms[i], i);
                        if (index == -1) {
                            index = i;
                        }
                    }
                }
                if (index != -1) {
                    TrySwitchFirearm(index);
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
            }
            m_CurrentFirearmIndex.OnValueChanged -= OnWeaponChanged;
            base.OnNetworkDespawn();
        }

        /// <summary>
        /// 当武器被添加到Loadout时，添加武器射击回调
        /// </summary>
        /// <param name="weapon"> 射击的武器 </param>
        /// <param name="idx"> 射击武器的下标 </param>
        void OnWeaponAdded(IWeapon weapon, int idx)
        {
            weapon.OnFire += () => OnWeaponFire(idx);
        }

        /// <summary>
        /// 将武器射击的本地Action进行广播
        /// 其实可以让武器自己广播
        /// </summary>
        /// <param name="idx"></param>
        void OnWeaponFire(int idx)
        {
            if (idx != CurrentFirearmIndex) {
                return;
            }
            EventManager.Broadcast(new WeaponFiredEvent());
        }

        void OnWeaponRemoved(IWeapon weapon)
        {
            // FIXME 这里应该删掉武器OnFire的回调
        }

        public override bool ValidActive() => m_Loadout != null && (m_Loadout.HasWeapon(1) || m_Loadout.HasWeapon(2));

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
        void TrySwitchFirearmServerRpc(int idx)
        {
            // 按下当前武器对应数字键收回武器
            if (CurrentFirearmIndex == idx) {
                m_CurrentFirearmIndex.Value = -1;
                Exit();
                return;
            }

            // 如果对应武器槽没有武器
            var newCurrentFirearm = m_Loadout.GetWeapon(idx);
            if (newCurrentFirearm == null) {
                return;
            }

            m_CurrentFirearmIndex.Value = idx;
        }

        public override bool TrySwitchFirearm(int idx)
        {
            int oldIdx = CurrentFirearmIndex;
            TrySwitchFirearmServerRpc(idx);
            return CurrentFirearmIndex != oldIdx;
            // Wait?
        }

        #endregion

        #region Reload

        [ServerRpc]
        void TryReloadServerRpc()
        {
            if (CurrentFirearmIndex == -1) {
                return;
            }
            if (!m_CurrentFirearm.ValidReload() || m_Inventory.GetAmount(m_CurrentFirearm.AmmoId) <= 0) {
                return;
            }
            int amount = m_CurrentFirearm.CurrentAmmo;
            int ammoId = m_CurrentFirearm.AmmoId;
            m_CurrentFirearm.StartReload();
            PutAmmoIntoInventory(ammoId, amount);
            m_ReloadCoroutine = StartCoroutine(ReloadCoroutine(m_CurrentFirearm.ReloadTime));
            StartReloadClientRpc();
        }

        [ClientRpc]
        void EndReloadClientRpc()
        {
            if(!IsOwner) {
                return;
            }
            EventManager.Broadcast(new WeaponEndReloadEvent {
                WeaponIdx = CurrentFirearmIndex
            });
        }
        [ClientRpc]
        void StartReloadClientRpc()
        {
            if (!IsOwner) {
                return;
            }
            EventManager.Broadcast(new WeaponStartReloadEvent {
                WeaponIdx = CurrentFirearmIndex
            });
            m_RuntimeData.AniParameter.Reload = true;
        }

        public override void TryReload()
        {
            if (CurrentFirearmIndex == -1) {
                return;
            }
            if (!m_CurrentFirearm.ValidReload() || m_Inventory.GetAmount(m_CurrentFirearm.AmmoId) <= 0) {
                return;
            }
            TryReloadServerRpc();
            
        }

        // 当通过在UI中拖动子弹到武器槽时执行
        void TryReload2(TryReloadEvent evt)
        {
            // FIXME: 界面拖动是有可能让当前未持有的武器换弹的，怎么办，要切枪吗
            TryReload();
        }

        void PutAmmoIntoInventory(int ammoId, int amount)
        {
            if (amount > 0) {
                m_Inventory.AddItem(ammoId, amount);
            }
        }

        int GetLoadAmmo(IWeapon weapon)
        {
            int ammoId = weapon.AmmoId;
            int ammoAmount = m_Inventory.GetAmount(ammoId);
            ammoAmount = Mathf.Min(ammoAmount, weapon.ClipAmmo);
            ammoAmount = m_Inventory.ReduceItemAmount(ammoId, ammoAmount);
            return ammoAmount;
        }
        IEnumerator ReloadCoroutine(float time)
        {
            m_Reloading.Value = true;
            yield return new WaitForSeconds(time);
            m_CurrentFirearm.EndReload(GetLoadAmmo(m_CurrentFirearm));
            m_Reloading.Value = false;

            m_ReloadCoroutine = null;
            EndReloadClientRpc();
        }

        #endregion

        // 非Owner检测到武器更换时同步更换
        void OnWeaponChanged(int pre, int cur)
        {
            // m_ReloadCoroutine修改起来太麻烦了，后续通过WeaponStateManager同步
            if (IsServer) {
                if(m_CurrentFirearm != null) {
                    m_CurrentFirearm.Attach(BackAttach);
                }

                if (m_ReloadCoroutine != null) {
                    StopCoroutine(m_ReloadCoroutine);
                    m_ReloadCoroutine = null;
                }
            }

            if(IsServer || IsOwner) {
                if (CurrentFirearmIndex == -1) {
                    if (IsOwner) {
                        m_CurrentFirearm.EndFire();
                        m_CurrentFirearm.OnUnequip();
                    }
                    m_CurrentFirearm = null;
                } else {
                    m_CurrentFirearm = m_Loadout.GetWeapon(cur);
                    if (IsServer) {
                        if (m_CurrentFirearm != null) {
                            m_CurrentFirearm.Attach(RightHandAttach);
                        }
                    }
                    if (IsOwner) {
                        m_CurrentFirearm.OnEquip();
                    }
                }
            }

            if (IsOwner) {
                EventManager.Broadcast(new WeaponChangedEvent { OldIdx = pre, NewIdx = CurrentFirearmIndex });
            }
        }
    }
}
