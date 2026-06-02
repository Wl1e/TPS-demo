using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class InventoryItemUI : MonoBehaviour, IDragable
    {
        public Image Icon;
        public TextMeshProUGUI Amount;
        public DragType Type => DragType.Inventory;
        public Image DragIcon => Icon;
        public InventorySlotUI Slot;
        public int SlotIdx => Slot.SlotIdx;
        int m_ItemId;
        public int ItemId => m_ItemId;

        public void UpdateIcon(int itemId)
        {
            m_ItemId = itemId;
            var icon = ItemUIUtils.GetItemSprite(m_ItemId);
            Icon.sprite = icon;
            Icon.enabled = icon != null;
        }

        public void UpdateAmount(int amount)
        {
            Amount.text = amount > 1 ? amount.ToString() : "";
        }

        public void UpdateIconAndAmount(int itemId, int amount)
        {
            UpdateIcon(itemId);
            UpdateAmount(amount);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DragManager.Instance.StartDrag(this);
            Icon.enabled = false;
            Amount.enabled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragManager.Instance.EndDrag();
            Icon.enabled = true;
            Amount.enabled = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }
    }
}
