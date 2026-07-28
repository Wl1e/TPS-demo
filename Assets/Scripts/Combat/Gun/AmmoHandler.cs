
using System;
using Unity.Netcode;

namespace TPSDemo
{

    [Serializable]
    public class AmmoHandler : NetworkBehaviour
    {

        /// <summary>
        /// 弹匣容量
        /// </summary>
        public int ClipSize => m_Weapon.Config.DefaultClipSize;

        /// <summary>
        /// 换弹时间
        /// </summary>
        public float ReloadTime => m_Weapon.Config.ReloadTime;

        /// <summary>
        /// 使用子弹ID
        /// </summary>
        public int AmmoId => m_Weapon.Config.AmmoId;
        /// <summary>
        /// 子弹数量
        /// </summary>
        public int CurrentAmmo => m_CurrentAmmo.Value;

        private IWeapon m_Weapon;
        private readonly NetworkVariable<int> m_CurrentAmmo = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if(IsOwner) {
                m_CurrentAmmo.OnValueChanged += AmmoChanged;
            }
        }

        public void Initialize(IWeapon weapon) => m_Weapon = weapon;

        public bool ValidReload()
        {
            return m_CurrentAmmo.Value < ClipSize;
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
            SetAmmo(ammo);
        }

        public void SetAmmo(int ammo) => m_CurrentAmmo.Value = ammo;

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
