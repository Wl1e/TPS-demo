
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TPSDemo
{
    public class AttachmentManager : MonoBehaviour
    {
        Dictionary<IAttachment.AttachmentSlot, IAttachment> m_Attachments;

        public Dictionary<IAttachment.AttachmentSlot, IAttachment> Attachments => m_Attachments;

        [Header(header: "配件挂点")]
        public Transform ScopeSocket;
        public Transform MagazineSocket;

        [Header(header: "默认配件")]
        public Scope DefaultScope;
        public Magazine DefaultMagazine;

        private void Awake()
        {
            m_Attachments = new Dictionary<IAttachment.AttachmentSlot, IAttachment>();
            if (DefaultScope != null) {
                m_Attachments[IAttachment.AttachmentSlot.Scope] = DefaultScope;
            }
            if (DefaultMagazine != null) {
                m_Attachments[IAttachment.AttachmentSlot.Magazine] = DefaultMagazine;
            }
        }

        public void AddAttachment(IAttachment attachment)
        {
            var slot = attachment.Slot;
            m_Attachments[slot] = attachment;
            attachment.SetParent(GetTargetSocket(slot));
            if (attachment.Slot == IAttachment.AttachmentSlot.Scope) {
                if (DefaultScope != null) {
                    DefaultScope.gameObject.SetActive(false);
                    //DefaultScope.Unequip();
                }
            } else if (attachment.Slot == IAttachment.AttachmentSlot.Magazine) {
                if (DefaultMagazine != null) {
                    DefaultMagazine.gameObject.SetActive(false);
                    //DefaultMagazine.Unequip();
                }
            }
        }
        public void RemoveAttachment(IAttachment attachment)
        {
            m_Attachments.Remove(attachment.Slot);
            if (attachment.Slot == IAttachment.AttachmentSlot.Scope) {
                if (DefaultScope != null) {
                    m_Attachments[IAttachment.AttachmentSlot.Scope] = DefaultScope;
                    DefaultScope.gameObject.SetActive(true);
                    //DefaultScope.SetParent(GetTargetSocket(IAttachment.AttachmentSlot.Scope));
                }

            } else if (attachment.Slot == IAttachment.AttachmentSlot.Magazine) {
                if (DefaultMagazine != null) {
                    m_Attachments[IAttachment.AttachmentSlot.Magazine] = DefaultMagazine;
                    DefaultMagazine.gameObject.SetActive(true);
                    //DefaultMagazine.SetParent(GetTargetSocket(IAttachment.AttachmentSlot.Magazine));
                }
            }
        }

        Transform GetTargetSocket(IAttachment.AttachmentSlot slot)
        {
            switch (slot) {
                case IAttachment.AttachmentSlot.Scope:
                    return ScopeSocket;
                case IAttachment.AttachmentSlot.Muzzle:
                    return null;
                case IAttachment.AttachmentSlot.Grip:
                    return null;
                case IAttachment.AttachmentSlot.Stock:
                    return null;
                case IAttachment.AttachmentSlot.Magazine:
                    return null;
            }
            return null;
        }

        public IAttachment GetAttachment(IAttachment.AttachmentSlot slot)
        {
            if (m_Attachments.TryGetValue(slot, out var attachment)) {
                return attachment;
            }
            return null;
        }
    }
}
