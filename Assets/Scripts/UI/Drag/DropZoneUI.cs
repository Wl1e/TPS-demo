using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DropZoneUI : MonoBehaviour, IDropTarget
    {
        [SerializeField] Image m_Background;
        [SerializeField] Color m_NormalColor = new Color(1f, 0.2f, 0.2f, 0.5f);
        [SerializeField] Color m_HoverColor = new Color(1f, 0.2f, 0.2f, 0.8f);
        public DragType Type => DragType.Inventory | DragType.Weapon;

        void Start()
        {
            gameObject.SetActive(false);
        }


        public void OnDrop(IDragable drag)
        {
        }

        void IDropTarget.OnDragEnter(IDragable source)
        {
            if (m_Background != null)
                m_Background.color = m_HoverColor;
        }

        void IDropTarget.OnDragExit(IDragable source)
        {
            if (m_Background != null)
                m_Background.color = m_NormalColor;
        }
    }
}
