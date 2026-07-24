
using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    [Serializable]
    public class AmmoHandler : NetworkBehaviour
    {
        /// <summary>
        /// 初始弹匣容量
        /// </summary>
        private int DefaultClipSize => m_Weapon.Config.DefaultClipSize;
        /// <summary>
        /// 换弹时间
        /// </summary>
        private float m_ReloadTime => m_Weapon.Config.ReloadTime;
        /// <summary>
        /// 使用子弹ID
        /// </summary>
        private int m_AmmoId => m_Weapon.Config.AmmoId;

        IWeapon m_Weapon;

        private int m_ClipSize;

        NetworkVariable<int> m_CurrentAmmo = new NetworkVariable<int>();
        public int CurrentAmmo => m_CurrentAmmo.Value;
        public float ReloadTime => m_ReloadTime;
        public int ClipSize => m_ClipSize;
        public int AmmoId => m_AmmoId;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // 子弹由Server写入
            if (IsServer) {
                m_CurrentAmmo.Value = m_ClipSize;
            }
            if(IsOwner) {
                m_CurrentAmmo.OnValueChanged += AmmoChanged;
            }
        }

        public void Initialize(IWeapon weapon)
        {
            m_Weapon = weapon;
            m_ClipSize = DefaultClipSize;
        }

        public bool ValidReload()
        {
            return m_CurrentAmmo.Value < m_ClipSize;
        }

        public void StartReload()
        {
            if (!ValidReload()) {
                return;
            }
            m_CurrentAmmo.Value = 0;
        }

        public void EndReload(int ammo)
        {
            m_CurrentAmmo.Value = ammo;
        }

        public bool ComsumeAmmo(int ammo = 1)
        {
            if (!EnoughAmmo(ammo)) {
                return false;
            }
            m_CurrentAmmo.Value -= ammo;
            return true;
        }

        public bool EnoughAmmo(int ammo = 1) {
            return m_CurrentAmmo.Value >= ammo;
        }

        private void AmmoChanged(int previousValue, int newValue)
        {
            EventManager.Broadcast(new Event.AmmoChangedEvent());
        }
    }
}
