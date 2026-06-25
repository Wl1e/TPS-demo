using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(menuName = "ItemData/ItemData", fileName = "ItemData")]
    public class ItemData: ScriptableObject
    {
        [Header("基础数据")]
        [Tooltip("道具ID")]
        public int Id;
        [Tooltip("道具名字")]
        public string Name;
        [Tooltip("道具类型")]
        public ItemType Type;
        [Tooltip("道具描述")]
        [TextArea]
        public string Description;
        [Tooltip("道具图标（显示在背包界面）")]
        public Sprite Icon;
        [Tooltip("最大堆叠数量")]
        public int MaxStack;
        [Tooltip("预制体")]
        public UnityEngine.AddressableAssets.AssetReference Prefab;
        [Tooltip("Pikcup预制体")]
        public UnityEngine.AddressableAssets.AssetReference PickupPrefab;

        [Tooltip("放置类型（UI）")]
        public UI.DragType DragType;
    }
}
