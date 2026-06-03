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
            var itemData = ItemDataList.GetItemData(itemId);
            if (itemData == null) {
                return DragType.None;
            }
            return itemData.DragType;
        }

        static public Sprite GetItemSprite(int itemId)
        {
            return ItemDataList.GetItemData(itemId)?.Icon;
        }
        static public ItemType GetItemType(int itemId)
        {
            return ItemDataList.GetItemData(itemId)?.Type ?? ItemType.None;
        }
    }
}
