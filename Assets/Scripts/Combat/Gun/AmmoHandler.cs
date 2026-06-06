
using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    [Serializable]
    public class AmmoHandler : NetworkBehaviour
    {
        [SerializeField] int m_ClipSize;
        [SerializeField] float m_ReloadTime;
        [SerializeField] int m_AmmoId;

        IWeapon m_Weapon;

        NetworkVariable<int> m_CurrentAmmo;
        public int CurrentAmmo => m_CurrentAmmo.Value;
        public float ReloadTime => m_ReloadTime;
        public int ClipSize => m_ClipSize;
        public int AmmoId => m_AmmoId;

        public void Awake()
        {
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
