
using UnityEngine;

namespace TPSDemo
{
    public abstract class AttachmentBase : MonoBehaviour, IAttachment
    {

        public IWeapon Weapon;

        [SerializeField] private IAttachment.AttachmentSlot m_Slot;

        [SerializeField] private Vector3 m_PositionOffset;
        [SerializeField] private Vector3 m_EulerOffset;

        [SerializeField] private ItemData m_ItemData;

        public IAttachment.AttachmentSlot Slot => m_Slot;
        public string Name => m_ItemData.Name;
        public int Id => m_ItemData.Id;


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
