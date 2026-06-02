using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] HealthUI m_HealthUI;
        [SerializeField] WeaponHUDUI m_WeaponUI;
        [SerializeField] CrosshairUI m_CrosshairUI;
        void Start()
        {
        }

        // Update is called once per frame
        public void Initialze()
        {
            m_HealthUI.Initialize();
            m_WeaponUI.Initialize();
            m_CrosshairUI.Initialize();
        }
    }
}
