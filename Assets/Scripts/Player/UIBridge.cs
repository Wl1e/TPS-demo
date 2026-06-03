
using UnityEngine;

namespace TPSDemo
{

    // 本来打算在逻辑层和UI层添加这个中间件，但是想了想，已经有EventManager了，费那么大劲干嘛
    public class UIBridge : MonoBehaviour
    {
        public FloatEvent OnHealthChanged;
        //public AmmoEvent OnAmmoChanged;

        TPSDemo.WeaponManager m_WeaponManager;

        void Start()
        {
            var health = GetComponent<Health>();
            if (health && OnHealthChanged != null) {
                health.OnTakeDamaged += (GameObject obj, float value) => OnHealthChanged.Raise(health.Ratio);
                health.OnHealed += (float value) => OnHealthChanged.Raise(health.Ratio);
            }

            //m_WeaponManager = GetComponent<WeaponManager>();
            //m_WeaponManager.OnWeaponReload += (bool reloadStarted, float time) => HandleAmmoChanged();
            //m_WeaponManager.OnWeaponFire += HandleAmmoChanged;
            //m_WeaponManager.Wea
        }

        //void HandleAmmoChanged()
        //{
        //    OnAmmoChanged.Raise(
        //        new AmmoData {
        //            MaxAmmo = m_WeaponManager.CurrentWeapon?.MaxAmmo ?? 0,
        //            CurrentAmmo = m_WeaponManager.CurrentWeapon?.CurrentAmmo ?? 0
        //        }
        //    );
        //}    
    }
}
