using UnityEngine;

namespace TPSDemo
{
    public enum ItemLevel
    {
        Common = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5
    }
    public interface IGear : IPickupable
    {
        public ItemLevel GearLevel { get; }
        public Color GearColor { get; }
    }

    public abstract class GearBase : MonoBehaviour, IGear
    {
        [SerializeField] string m_GearName;
        [SerializeField] string m_Description;
        [SerializeField] Sprite m_Icon;
        [SerializeField] ItemLevel m_Level;

        public float InteractRadius => 3f;

        public ItemLevel GearLevel => m_Level;
        public Color GearColor => GetColor(m_Level);
        public string Name => m_GearName;
        public string Description => m_Description;
        public Sprite Icon => m_Icon;

        static private Color GetColor(ItemLevel level)
        {
            switch (level) {
                case ItemLevel.Common:
                    return Color.white;
                case ItemLevel.Rare:
                    return Color.blue;
                case ItemLevel.Epic:
                    return Color.purple;
                case ItemLevel.Legendary:
                    return Color.gold;
                case ItemLevel.Mythic:
                    return Color.red;
            }
            return Color.black;
        }

        public abstract string Hint { get; }
        public abstract float HoldDuration { get; }
        public abstract void OnInteractPress(GameObject interactor);
        public abstract void OnInteractHold(GameObject interactor);
        public abstract void OnInteractRelease(GameObject interactor, bool completed);
        public abstract void Drop(GameObject obj);
    }

    public class EvoShield : GearBase
    {
        [SerializeField] float m_Shield;
        [SerializeField] float m_EVOPoint = 0f;

        public override string Hint => "EvoShield";
        public override float HoldDuration => 0f;
        public override void OnInteractPress(GameObject interactor)
        {
            if (interactor.TryGetComponent<Shield>(out var shield)) {
                shield.SetMaxShield(m_Shield);
            }
        }
        public override void OnInteractHold(GameObject interactor)
        { }
        public override void OnInteractRelease(GameObject interactor, bool completed)
        { }

        public override void Drop(GameObject obj)
        {
        }
    }
}
