
using UnityEngine;

namespace TPSDemo
{
    public abstract class AttachmentBase : MonoBehaviour, IAttachment
    {

        public IWeapon Weapon;

        [SerializeField] IAttachment.AttachmentSlot m_Slot;
        public IAttachment.AttachmentSlot Slot => m_Slot;

        [SerializeField] Vector3 m_PositionOffset;
        [SerializeField] Vector3 m_EulerOffset;

        [SerializeField] string m_AttachmentName;
        [SerializeField] int m_Id;

        public string Name => m_AttachmentName;
        public int Id => m_Id;


        public void Equip(IWeapon weapon)
        {
            Weapon = weapon;
            OnEquip();
        }
        public void Unequip()
        {
            OnUnequip();
            Weapon = null;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public abstract void OnEquip();
        public abstract void OnUnequip();
        public abstract void OnAim();
        public abstract void OnFire();
        public void SetParent(Transform parent)
        {
            transform.SetParent(parent, false);
            transform.position += m_PositionOffset;
            transform.Rotate(m_EulerOffset);
        }
    }
}
