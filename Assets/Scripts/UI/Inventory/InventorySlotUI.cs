using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventorySlotUI : MonoBehaviour, IDropTarget
    {
        public GameObject ItemPrefab;

        public Image Icon;
        public InventoryItemUI Item;
        public InventoryUI Inventory;
        public int SlotIdx;

        public DragType Type => DragType.Inventory;
        public void OnDrop(IDragable drag)
        {
            Inventory.Swap(SlotIdx, drag.SlotIdx);
        }
        public void InitializeItem()
        {
            var itemObj = Instantiate(ItemPrefab, transform);
            Item = itemObj.GetComponent<InventoryItemUI>();
            Item.Slot = this;
        }

        public void DestroyItem()
        {
            if(Item) {
                Destroy(Item.gameObject);
            }
        }

        public void OnDragEnter(IDragable obj)
        {
        }
        public void OnDragExit(IDragable obj)
        {
        }
    }
}
