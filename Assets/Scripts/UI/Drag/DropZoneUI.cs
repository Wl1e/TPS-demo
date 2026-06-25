using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class DropZoneUI : MonoBehaviour, IDropTarget
    {
        [SerializeField] Image m_Background;
        [SerializeField] Color m_NormalColor = new(1f, 0.2f, 0.2f, 0.5f);
        [SerializeField] Color m_HoverColor = new(1f, 0.2f, 0.2f, 0.8f);
        public DragType Type => DragType.Inventory | DragType.Weapon;

        void Start()
        {
            if (m_Background != null) {
                m_Background.color = m_NormalColor;
            }
            gameObject.SetActive(false);
        }


        public void OnDrop(IDragable drag)
        {
            if (drag.Resource == DragResource.Inventory) {
                EventManager.Broadcast(new Event.InventoryDropItemEvent { Slot = drag.SlotIdx });
            } else if(drag.Resource == DragResource.Loadout) {
                EventManager.Broadcast(new Event.TryUnequipWeaponEvent { WeaponIdx = drag.SlotIdx });
            }
        }

        void IDropTarget.OnDragEnter(IDragable source)
        {
            if (m_Background != null) {
                m_Background.color = m_HoverColor;
            }
        }

        void IDropTarget.OnDragExit(IDragable source)
        {
            if (m_Background != null) {
                m_Background.color = m_NormalColor;
            }
        }
    }
}
