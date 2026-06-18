
using System.Collections.Generic;
using System.Net.Mail;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class AttachmentManager : NetworkBehaviour
    {
        Dictionary<IAttachment.AttachmentSlot, IAttachment> m_Attachments = new();

        public Dictionary<IAttachment.AttachmentSlot, IAttachment> Attachments => m_Attachments;

        [Header("配件挂点")]
        [Tooltip("瞄准镜挂点")]
        public Transform ScopeSocket;
        [Tooltip("弹匣挂点")]
        public Transform MagazineSocket;

        [Header("默认配件")]
        [Tooltip("默认瞄准镜")]
        public Scope DefaultScope;
        [Tooltip("默认弹匣")]
        public Magazine DefaultMagazine;

        // NetWorkVariable
        private NetworkVariable<int> m_ScopeNV = new();
        private NetworkVariable<int> m_MagazineNV = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // 不把默认挂件放在m_Attachments，简化
            //if (DefaultScope != null) {
            //    m_Attachments[IAttachment.AttachmentSlot.Scope] = DefaultScope;
            //}
            //if (DefaultMagazine != null) {
            //    m_Attachments[IAttachment.AttachmentSlot.Magazine] = DefaultMagazine;
            //}
            m_ScopeNV.OnValueChanged += OnScopeChanged;
            m_MagazineNV.OnValueChanged += OnMagazineChanged;
        }

        public override void OnNetworkDespawn()
        {
            m_ScopeNV.OnValueChanged -= OnScopeChanged;
            m_MagazineNV.OnValueChanged -= OnMagazineChanged;
            base.OnNetworkDespawn();
        }

        public void AddAttachment(IAttachment.AttachmentSlot slot, int attachmentId)
        {
            if(!IsOwner) {
                return;
            }
            AddAttachmentServerRpc(slot, attachmentId);
        }

        public void RemoveAttachment(IAttachment.AttachmentSlot slot)
        {
            if (!IsOwner) {
                return;
            }
            RemoveAttachmentServerRpc(slot);
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

        #region Server

        [ServerRpc]
        private void AddAttachmentServerRpc(IAttachment.AttachmentSlot slot, int attachmentId)
        {
            if (slot == IAttachment.AttachmentSlot.Scope) {
                m_ScopeNV.Value = attachmentId;
            } else if(slot == IAttachment.AttachmentSlot.Magazine) {
                m_MagazineNV.Value = attachmentId;
            }
        }

        [ServerRpc]
        private void RemoveAttachmentServerRpc(IAttachment.AttachmentSlot slot)
        {
            if (slot == IAttachment.AttachmentSlot.Scope) {
                m_ScopeNV.Value = -1;
            } else if (slot == IAttachment.AttachmentSlot.Magazine) {
                m_MagazineNV.Value = -1;
            }
        }

        #endregion


        private void OnScopeChanged(int oldId, int newId) => OnAttachmentChanged(IAttachment.AttachmentSlot.Scope, oldId, newId);
        private void OnMagazineChanged(int oldId, int newId) => OnAttachmentChanged(IAttachment.AttachmentSlot.Magazine, oldId, newId);

        private void OnAttachmentChanged(IAttachment.AttachmentSlot slot, int oldId, int newId)
        {
            bool Unequip = newId == -1;

            if(oldId != -1) {
                OnRemoveAttachment(slot, oldId, newId == -1);
            }

            if (newId != -1) {
                OnAddAttachment(slot, newId);
            } 
        }

        private void OnAddAttachment(IAttachment.AttachmentSlot slot, int attachmentId)
        {
            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(attachmentId);
            var attachment = WorldItemManager.CreateItemGO<IAttachment>(itemData);
            m_Attachments[slot] = attachment;
            attachment.SetParent(GetTargetSocket(slot));
        }

        private void OnRemoveAttachment(IAttachment.AttachmentSlot slot, int attachmentId, bool enableDefault)
        {
            var attachment = m_Attachments[slot];
            m_Attachments[slot] = null;
            if (attachment != null) {
                attachment.Destroy();
            }

            // FIXME: 不要往地上扔，往背包扔
            var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(attachmentId);
            WorldItemManager.Instance.SpawnItem(itemData, transform.position);

            if (enableDefault) {
                if (slot == IAttachment.AttachmentSlot.Scope) {
                    if (DefaultScope != null) {
                        //m_Attachments[IAttachment.AttachmentSlot.Scope] = DefaultScope;
                        DefaultScope.gameObject.SetActive(true);
                        //DefaultScope.SetParent(GetTargetSocket(IAttachment.AttachmentSlot.Scope));
                    }

                } else if (slot == IAttachment.AttachmentSlot.Magazine) {
                    if (DefaultMagazine != null) {
                        //m_Attachments[IAttachment.AttachmentSlot.Magazine] = DefaultMagazine;
                        DefaultMagazine.gameObject.SetActive(true);
                        //DefaultMagazine.SetParent(GetTargetSocket(IAttachment.AttachmentSlot.Magazine));
                    }
                }
            }
        }

        // For UI
        public List<(IAttachment.AttachmentSlot, int)> GetAttachmentList()
        {
            var list = new List<(IAttachment.AttachmentSlot, int)> {
                (
                    IAttachment.AttachmentSlot.Scope,
                    m_Attachments.GetValueOrDefault(IAttachment.AttachmentSlot.Scope, DefaultScope).Id
                ),
                (
                    IAttachment.AttachmentSlot.Magazine,
                    m_Attachments.GetValueOrDefault(IAttachment.AttachmentSlot.Magazine, DefaultMagazine).Id
                )
            };

            return list;
        }
    }
}
