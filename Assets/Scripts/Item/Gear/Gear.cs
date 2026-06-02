using UnityEngine;
using static IGear;

public interface IGear: IPickupable
{
    public enum Level
    {
        Common = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5
    }
    public Level GearLevel { get; }
    public Color GearColor { get; }
}

public abstract class GearBase: MonoBehaviour, IGear
{
    [SerializeField] string m_GearName;
    [SerializeField] string m_Description;
    [SerializeField] Sprite m_Icon;
    [SerializeField]  Level m_Level;

    public float InteractRadius => 3f;

    public Level GearLevel => m_Level;
    public Color GearColor => GetColor(m_Level);
    public string Name => m_GearName;
    public string Description => m_Description;
    public Sprite Icon => m_Icon;

    static private Color GetColor(Level level)
    {
        switch (level) {
            case Level.Common:
                return Color.white;
            case Level.Rare:
                return Color.blue;
            case Level.Epic:
                return Color.purple;
            case Level.Legendary:
                return Color.gold;
            case Level.Mythic:
                return Color.red;
        }
        return Color.black;
    }

    public abstract void Interact(GameObject obj);
    public abstract void Drop(GameObject obj);
    public void WhenSee()
    { }
}

public class EvoShield: GearBase
{
    [SerializeField] float m_Shield;
    [SerializeField] float m_EVOPoint = 0f;
    public override void Interact(GameObject obj)
    {
        if(obj.TryGetComponent<Shield>(out var shield)) {
            shield.SetMaxShield(m_Shield);
        }
    }

    public override void Drop(GameObject obj)
    {
    }
}
