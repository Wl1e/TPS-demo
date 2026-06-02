
using UnityEngine;
using static IAttachment;

public abstract class AttachmentBase: MonoBehaviour, IAttachment
{

    public IWeapon Weapon;

    [SerializeField] AttachmentSlot m_Slot;
    public AttachmentSlot Slot => m_Slot;

    [SerializeField] Vector3 m_PositionOffset;
    [SerializeField] Vector3 m_EulerOffset;

    [SerializeField] string m_AttachmentName;
    [SerializeField] int m_Id;

    public string Name => m_AttachmentName;
    public int Id => m_Id;


    public void Equip(IWeapon weapon)
    {
        Weapon = weapon;
        Weapon.AddAttachment(this);
        OnEquip();
    }
    public void Unequip()
    {
        Weapon.RemoveAttachment(this);
        OnUnequip();
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
