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
        public Image Grip;
        public Image Laser;
        public Image Muzzle;
        public int WeaponId;

        public DragResource Resource => DragResource.Loadout;
        public DragType Type { get; private set; }
        public Image DragIcon => Base;
        public WeaponSlotUI Slot;   
        public int SlotIdx => Slot.SlotIdx;

        int IDragable.ItemId => WeaponId;

        public void UpdateInfo(int weaponId, List<(IAttachment.AttachmentSlot, int)> attachmentList)
        {
            WeaponId = weaponId;
            var icon = ItemUIUtils.GetItemIcon(WeaponId);
            Base.sprite = icon;
            Base.enabled = icon != null;
            Type = ItemUIUtils.GetDragType(WeaponId);

            SetAttachmentEnable(false);

            foreach (var attachment in attachmentList) {
                SetAttachmentSprite(attachment.Item1, attachment.Item2);
            }
        }

        void SetAttachmentSprite(IAttachment.AttachmentSlot slot, int attachmentId)
        {
            var sprite = ItemUIUtils.GetAttachmentSprite(WeaponId, attachmentId);
            if (sprite == null) {
                Debug.LogWarning($"attachment{attachmentId} dont have Icon");
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
                case IAttachment.AttachmentSlot.Grip:
                    Grip.sprite = sprite;
                    Grip.enabled = true;
                    break;
                case IAttachment.AttachmentSlot.Muzzle:
                    Muzzle.sprite = sprite;
                    Muzzle.enabled = true;
                    break;
                case IAttachment.AttachmentSlot.Laser:
                    Laser.sprite = sprite;
                    Laser.enabled = true;
                    break;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DragManager.Instance.StartDrag(this);
            Base.enabled = false;

            SetAttachmentEnable(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragManager.Instance.EndDrag();
            Base.enabled = true;

            SetAttachmentEnable(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        private void SetAttachmentEnable(bool enable)
        {
            void f(Image image)
            {
                if (image.sprite != null) {
                    image.enabled = enable;
                } else {
                    image.enabled = false;
                }
            }
            f(Scope);
            f(Magazine);
            f(Grip);
            f(Laser);
            f(Muzzle);
        }
    }
}
