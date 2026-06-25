using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TPSDemo.UI
{
    public enum DragResource
    {
        Inventory,
        Loadout
    };

    public interface IDragable:
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public DragResource Resource { get; }
        public DragType Type { get; }
        public Image DragIcon { get; }
        public int SlotIdx { get; }
        public int ItemId { get; }
    }
}
