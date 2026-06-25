using UnityEngine;

// 后续可能需要子类型，直接在这加就行了 xxxAmmo xxxWeapon

namespace TPSDemo
{

    public enum ItemType
    {
        None,
        Weapon,
        Ammo,
        Grenade,
        Attachment,
    }

    public interface IItem
    {
        public int Id { get; }
        public string Name { get; }
        public ItemType Type { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public ItemData Data { get; }
    }

    public abstract class ItemBase : MonoBehaviour, IItem, IStackable
    {
        [SerializeField] protected ItemData m_Data;
        [SerializeField] protected int m_Amount;
        public int Id => m_Data.Id;
        public string Name => m_Data.Name;
        public ItemType Type => m_Data.Type;
        public string Description => m_Data.Description;
        public Sprite Icon => m_Data.Icon;
        public ItemData Data => m_Data;
        public int MaxStack => m_Data.MaxStack;
        public int Amount => m_Amount;
    }
}
