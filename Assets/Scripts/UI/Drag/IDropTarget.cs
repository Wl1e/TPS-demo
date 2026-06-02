using UnityEngine.EventSystems;

namespace UI
{
    public interface IDropTarget
    {
        public DragType Type { get; }
        public void OnDragEnter(IDragable drag);
        public void OnDragExit(IDragable drag);
        public void OnDrop(IDragable drag);
    }
}
