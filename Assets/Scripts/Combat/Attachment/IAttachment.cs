
using UnityEngine;

public interface IAttachment
{
    public string Name { get; }
    public int Id { get; }
    public enum AttachmentSlot { Scope, Muzzle, Grip, Stock, Magazine }
    public AttachmentSlot Slot { get; }
    public void Equip(IWeapon weapon);
    public void Unequip();
    public void SetParent(Transform parent);
}

