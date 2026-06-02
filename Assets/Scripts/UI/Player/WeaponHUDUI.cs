using UnityEngine;
using UnityEngine.UI;

using WeaponStartReloadEvent = Event.WeaponStartReloadEvent;
using WeaponEndReloadEvent = Event.WeaponEndReloadEvent;
using WeaponChangedEvent = Event.WeaponChangedEvent;
using TMPro;
using Event;

namespace UI
{
    public class WeaponHUDUI : MonoBehaviour
    {
        [Header("Weapon Tabs")]
        [SerializeField] private Image[] m_TabBackgrounds;
        [SerializeField] private Color m_ActiveColor = Color.white;
        [SerializeField] private Color m_InactiveColor = new Color(1f, 1f, 1f, 0.4f);
        [SerializeField] private Image[] m_WeaponIcons;

        [Header("Ammo Display")]
        [SerializeField] private TextMeshProUGUI m_AmmoText;

        bool m_IsActive = false;

        void OnEnable()
        {
            EventManager.AddListener<WeaponStartReloadEvent>(HandleStartReload);
            EventManager.AddListener<WeaponEndReloadEvent>(HandleEndReload);
            EventManager.AddListener<WeaponChangedEvent>(HandleWeaponChanged);
            EventManager.AddListener<WeaponFiredEvent>(HandleWeaponFire);
            EventManager.AddListener<InventoryUpdateEvent>(HandleInventoryUpdate);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<WeaponStartReloadEvent>(HandleStartReload);
            EventManager.RemoveListener<WeaponEndReloadEvent>(HandleEndReload);
            EventManager.RemoveListener<WeaponChangedEvent>(HandleWeaponChanged);
            EventManager.RemoveListener<WeaponFiredEvent>(HandleWeaponFire);
            EventManager.RemoveListener<InventoryUpdateEvent>(HandleInventoryUpdate);
        }

        public void Initialize()
        {
            UpdateWeaponTabs(PlayerDataProxy.Instance.GetCurrentFirearmIdx());
            UpdateAmmoDisplay();
            m_IsActive = true;
        }

        void HandleWeaponChanged(WeaponChangedEvent evt)
        {
            // WeaponManager会在Start时加入初始武器触发事件
            if (!m_IsActive) {
                return;
            }
            UpdateWeaponTabs(evt.NewIdx);
            UpdateAmmoDisplay();
        }

        void HandleStartReload(WeaponStartReloadEvent evt)
        {
            UpdateAmmoDisplay();
        }
        void HandleEndReload(WeaponEndReloadEvent evt)
        {
            UpdateAmmoDisplay();
        }
        void HandleWeaponFire(WeaponFiredEvent evt)
        {
            UpdateAmmoDisplay();
        }
        void HandleInventoryUpdate(InventoryUpdateEvent evt)
        {
            UpdateAmmoDisplay();
        }

        void UpdateWeaponTabs(int currentIdx)
        {
            var weapon = PlayerDataProxy.Instance.GetCurrentFirearm();
            if (currentIdx < 0 || weapon == null) {
                return;
            }

            for (int i = 0; i < m_TabBackgrounds.Length; i++) {
                if (m_TabBackgrounds[i] != null)
                    m_TabBackgrounds[i].color = (i == currentIdx) ? m_ActiveColor : m_InactiveColor;
            }

            //for (int i = 0; i < m_WeaponIcons.Length; i++) {
            //    if (m_WeaponIcons[i] == null)
            //        continue;
            //    if (i < m_WeaponManager.WeaponCount) {
            //        m_WeaponIcons[i].gameObject.SetActive(true);
            //        Weapon weapon = m_WeaponManager.GetWeapon(i) as Weapon;
            //        if (weapon != null && weapon.Base != null)
            //            m_WeaponIcons[i].sprite = weapon.Base;
            //    } else {
            //        m_WeaponIcons[i].gameObject.SetActive(false);
            //    }
            //}
        }

        void UpdateAmmoDisplay()
        {
            var current = PlayerDataProxy.Instance.GetCurrentFirearm()?.CurrentAmmo ?? 0;
            var max = PlayerDataProxy.Instance.GetInventoryAmountByType(ItemType.Ammo);
            m_AmmoText.text = $"{current} / {max}";
        }
    }

}
