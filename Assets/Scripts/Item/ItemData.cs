using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(menuName = "ItemData/ItemData", fileName = "ItemData")]
    public class ItemData: ScriptableObject
    {
        public int Id;
        public string Name;
        public ItemType Type;
        [TextArea]
        public string Description;
        public Sprite Icon;
        public int MaxStack;
        public GameObject Prefab;

        public UI.DragType DragType;
    }
}
