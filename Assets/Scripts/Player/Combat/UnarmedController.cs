using UnityEngine;

namespace TPSDemo
{

    public class UnarmedController : CombatSlot
    {
        bool m_IsActive = false;
        public override bool IsActive => m_IsActive;
        public override bool ValidActive()
        {
            return true;
        }
        public override void SetActive(bool isActive)
        {
            m_IsActive = isActive;
        }
        public override void Attack(bool isEnd)
        {
        }

        public override bool ValidAim()
        {
            return false;
        }

        public override void OnAim(bool isAiming)
        {
        }
    }
}
