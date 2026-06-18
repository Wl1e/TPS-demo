using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class WeaponItemUI : MonoBehaviour, IDragable
    {
        public Image Base;
        public Image Scope;
        public Image Magazine;
        private List<Image> m_Attachments;
        public int WeaponId;

        public DragType Type { get; private set; }
        public Image DragIcon => Base;
        public WeaponSlotUI Slot;   
        public int SlotIdx => Slot.SlotIdx;

        int IDragable.ItemId => WeaponId;

        private void Awake()
        {
            m_Attachments = new List<Image>{
                Scope,
                Magazine
            };
        }

        public void UpdateInfo(int itemId, List<(IAttachment.AttachmentSlot, int)> attachmentList)
        {
            WeaponId = itemId;
            var icon = ItemUIUtils.GetItemSprite(WeaponId);
            Base.sprite = icon;
            Base.enabled = icon != null;
            Type = ItemUIUtils.GetDragType(WeaponId);

            print("m_Attachments: " + m_Attachments);

            UpdateAttachmentImage(false);

            foreach (var attachment in attachmentList) {
                SetAttachmentSprite(attachment.Item1, attachment.Item2);
            }
        }

        void SetAttachmentSprite(IAttachment.AttachmentSlot slot, int attachmentId)
        {
            var sprite = ItemUIUtils.GetItemSprite(attachmentId);
            if (sprite == null) {
                print($"attachment{attachmentId} dont have Icon");
                return;
            }
            switch (slot) {
                case IAttachment.AttachmentSlot.Scope:
                    Scope.sprite = sprite;
                    Scope.enabled = true;
                    break;
                case IAttachment.AttachmentSlot.Magazine:
                    Magazine.sprite = sprite;
                    Magazine.enabled = true;
                    break;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DragManager.Instance.StartDrag(this);
            Base.enabled = false;

            UpdateAttachmentImage(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragManager.Instance.EndDrag();
            Base.enabled = true;

            UpdateAttachmentImage(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        private void UpdateAttachmentImage(bool enable)
        {
            if (Scope.sprite != null) {
                Scope.enabled = enable;
            } else {
                Scope.enabled = false;
            }
            if (Magazine.sprite != null) {
                Magazine.enabled = enable;
            } else {
                Magazine.enabled = false;
            }
        }
    }
}
