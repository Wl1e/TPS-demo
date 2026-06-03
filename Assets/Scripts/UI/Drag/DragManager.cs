using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace TPSDemo.UI
{
    public class DragManager : Singleton<DragManager>
    {
        [SerializeField] Image m_DragSprite;
        [SerializeField] DropZoneUI m_DropZone;
        [SerializeField] GraphicRaycaster m_Raycaster;

        IDragable m_Drag;
        IDropTarget m_Target;
        bool m_IsDragging;

        PointerEventData m_PointerData;
        List<RaycastResult> m_RaycastResults = new List<RaycastResult>();

        public bool IsDragging => m_IsDragging;

        private void Start()
        {
            m_PointerData = new PointerEventData(EventSystem.current);
            m_DragSprite.gameObject.SetActive(false);
            m_DropZone.gameObject.SetActive(false);
        }

        public void StartDrag(IDragable dragable)
        {
            m_Drag = dragable;
            m_DragSprite.sprite = dragable.DragIcon.sprite;

            m_DragSprite.gameObject.SetActive(true);
            m_DropZone.gameObject.SetActive(true);

            m_IsDragging = true;
        }

        public void EndDrag()
        {
            if (!m_IsDragging) {
                return;
            }

            m_IsDragging = false;

            if (m_Target != null)
            {
                m_Target.OnDragExit(m_Drag);
                m_Target.OnDrop(m_Drag);
                m_Target = null;
            }

            m_DragSprite.gameObject.SetActive(false);
            m_DropZone.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!m_IsDragging) {
                return;
            }

            m_PointerData.position = Mouse.current.position.ReadValue();
            m_DragSprite.transform.position = m_PointerData.position;
            UpdateHover();
        }

        void UpdateHover()
        {
            var target = GetRaycastTarget();

            if (target != m_Target)
            {
                m_Target?.OnDragExit(m_Drag);
                m_Target = target;
                m_Target?.OnDragEnter(m_Drag);
            }
        }

        IDropTarget GetRaycastTarget()
        {
            m_RaycastResults.Clear();
            m_Raycaster.Raycast(m_PointerData, m_RaycastResults);

            IDropTarget found = null;
            foreach (var result in m_RaycastResults) {
                found = result.gameObject.GetComponent<IDropTarget>();
                if (found != null && CanDrop(found, m_Drag))
                    break;
            }
            return found;
        }

        bool CanDrop(IDropTarget drop, IDragable drag)
        {
            return (drop.Type | drag.Type) != DragType.None;
        }
    }
}
