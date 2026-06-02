using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    public interface IDragable:
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public DragType Type { get; }
        public Image DragIcon { get; }
        public int SlotIdx { get; }
        public int ItemId { get; }
    }
}
