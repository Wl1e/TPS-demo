
using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    [Serializable]
    public class AmmoHandler : NetworkBehaviour
    {
        [Tooltip("初始弹匣容量")]
        public int DefaultClipSize;
        [Tooltip("换弹时间")]
        [SerializeField] float m_ReloadTime;
        [Tooltip("使用子弹ID")]
        [SerializeField] int m_AmmoId;

        IWeapon m_Weapon;

        private int m_ClipSize;

        NetworkVariable<int> m_CurrentAmmo = new NetworkVariable<int>();
        public int CurrentAmmo => m_CurrentAmmo.Value;
        public float ReloadTime => m_ReloadTime;
        public int ClipSize => m_ClipSize;
        public int AmmoId => m_AmmoId;

        private void Awake()
        {
            m_ClipSize = DefaultClipSize;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // 子弹由Server写入
            if (IsServer) {
                m_CurrentAmmo.Value = m_ClipSize;
            }
        }

        public void Initialize(IWeapon weapon)
        {
            m_Weapon = weapon;
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
    }
}
