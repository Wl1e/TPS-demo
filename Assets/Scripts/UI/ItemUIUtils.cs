using System;
using System.Collections;
using UnityEngine;

namespace TPSDemo.UI
{
    [Flags]
    public enum DragType: int
    {
        None = 0,
        Inventory = 1 << 0,
        Weapon = 1 << 1
    };
    static public class ItemUIUtils
    {
        static public DragType GetDragType(int itemId)
        {
            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(itemId);
            if (itemData == null) {
                return DragType.None;
            }
            return itemData.DragType;
        }

        public static Sprite GetItemIcon(int itemId) =>
            ResourceManager.Instance.GetResource<ItemDataList>("ItemData")
                .GetItemData(itemId)?.Icon;
        public static Sprite GetAttachmentSprite(int weaponId, int attachmentId)
        {
            var weaponData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData")
                .GetItemData(weaponId);
            var attachmentData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData")
                .GetItemData(attachmentId);
            if(!weaponData || !attachmentData) {
                return null;
            }
            return ResourceManager.Instance.GetResource<AttachmentSpriteMap>("AttachmentSprite")
                .GetSprite(weaponData, attachmentData);
        }
        static public ItemType GetItemType(int itemId) =>
            ResourceManager.Instance.GetResource<ItemDataList>("ItemData")
                .GetItemData(itemId)?.Type ?? ItemType.None;
    }
}
