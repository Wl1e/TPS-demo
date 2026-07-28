
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class AttachmentManager : NetworkBehaviour
    {
        readonly Dictionary<IAttachment.AttachmentSlot, IAttachment> m_Attachments = new();

        public Dictionary<IAttachment.AttachmentSlot, IAttachment> Attachments => m_Attachments;

        [Header("配件挂点")]
        [Tooltip("瞄准镜挂点")]
        public Transform ScopeSocket;
        [Tooltip("弹匣挂点")]
        public Transform MagazineSocket;
        [Tooltip("激光挂点")]
        public Transform LaserSocket;
        [Tooltip("握把挂点")]
        public Transform GripSocket;
        [Tooltip("枪管挂点")]
        public Transform MuzzleSocket;


        [Header("默认配件")]
        [Tooltip("默认瞄准镜")]
        public ScopeAttachment DefaultScope = null;
        [Tooltip("默认弹匣")]
        public MagazineAttachment DefaultMagazine = null;
        [Tooltip("默认握把")]
        public ScopeAttachment DefaultGrip = null;
        [Tooltip("默认枪管")]
        public MagazineAttachment DefaultMuzzle = null;
        [Tooltip("默认镭射")]
        public ScopeAttachment DefaultLaser = null;

        // NetWorkVariable
        private NetworkVariable<int> m_ScopeNV = new(-1);
        private NetworkVariable<int> m_MagazineNV = new(-1);
        private NetworkVariable<int> m_LaserNV = new(-1);
        private NetworkVariable<int> m_GripNV = new(-1);
        private NetworkVariable<int> m_MuzzleNV = new(-1);

        public event Action OnAttachmentChanged;

        public IWeapon Weapon { get; set; }

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
            m_ScopeNV.OnValueChanged += ScopeChanged;
            m_MagazineNV.OnValueChanged += MagazineChanged;
            m_LaserNV.OnValueChanged += LaserChanged;
            m_GripNV.OnValueChanged += GripChanged;
            m_MuzzleNV.OnValueChanged += MuzzleChanged;
        }

        public override void OnNetworkDespawn()
        {
            m_ScopeNV.OnValueChanged -= ScopeChanged;
            m_MagazineNV.OnValueChanged -= MagazineChanged;
            m_LaserNV.OnValueChanged -= LaserChanged;
            m_GripNV.OnValueChanged -= GripChanged;
            m_MuzzleNV.OnValueChanged -= MuzzleChanged;
            base.OnNetworkDespawn();
        }

        public bool SupportAttachment(IAttachment.AttachmentSlot slot, int attachmentId)
        {
            return GetTargetSocket(slot) != null;
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

        private Transform GetTargetSocket(IAttachment.AttachmentSlot slot)
        {
            switch (slot) {
                case IAttachment.AttachmentSlot.Scope:
                    return ScopeSocket;
                case IAttachment.AttachmentSlot.Muzzle:
                    return MuzzleSocket;
                case IAttachment.AttachmentSlot.Grip:
                    return GripSocket;
                case IAttachment.AttachmentSlot.Laser:
                    return LaserSocket;
                case IAttachment.AttachmentSlot.Magazine:
                    return MagazineSocket;
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
            } else if (slot == IAttachment.AttachmentSlot.Magazine) {
                m_MagazineNV.Value = attachmentId;
            } else if (slot == IAttachment.AttachmentSlot.Grip) {
                m_GripNV.Value = attachmentId;
            } else if (slot == IAttachment.AttachmentSlot.Laser) {
                m_LaserNV.Value = attachmentId;
            } else if(slot == IAttachment.AttachmentSlot.Muzzle) {
                m_MuzzleNV.Value = attachmentId;
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


        private void ScopeChanged(int oldId, int newId) => AttachmentChanged(IAttachment.AttachmentSlot.Scope, oldId, newId);
        private void MagazineChanged(int oldId, int newId) => AttachmentChanged(IAttachment.AttachmentSlot.Magazine, oldId, newId);
        private void LaserChanged(int oldId, int newId) => AttachmentChanged(IAttachment.AttachmentSlot.Laser, oldId, newId);
        private void GripChanged(int oldId, int newId) => AttachmentChanged(IAttachment.AttachmentSlot.Grip, oldId, newId);
        private void MuzzleChanged(int oldId, int newId) => AttachmentChanged(IAttachment.AttachmentSlot.Muzzle, oldId, newId);

        private void AttachmentChanged(IAttachment.AttachmentSlot slot, int oldId, int newId)
        {
            bool unequip = (newId == -1);

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
            if (!itemData.IsNetCodePrefab) {
                StartCoroutine(
                    AssetCache.GetOrLoad(itemData.Prefab, obj => {
                        Debug.Log($"配件 {obj} 加载完成");
                         var attachment = obj.GetComponent<IAttachment>();
                        m_Attachments[slot] = attachment;
                         attachment.SetParent(GetTargetSocket(slot));
                        if (IsOwner) {
                            OnAttachmentChanged?.Invoke();
                        }
                    })
                );
            }
        }

        private void OnRemoveAttachment(IAttachment.AttachmentSlot slot, int attachmentId, bool enableDefault)
        {
            var attachment = GetAttachment(slot);
            m_Attachments[slot] = null;
            if (attachment != null) {
                attachment.Destroy();
                if (IsOwner) {
                    // var itemData = ResourceManager.Instance.GetResource<ItemDataList>("ItemData").GetItemData(attachmentId);
                    // WorldItemManager.Instance.SpawnItem(itemData, transform.position, 1);
                    var player = Weapon.Owner.GetComponent<PlayerController>();
                    player.Inventory.AddItem(attachmentId, 1);
                    OnAttachmentChanged?.Invoke();
                }
            }

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
            var list = new List<(IAttachment.AttachmentSlot, int)>();

            void f(IAttachment.AttachmentSlot slot, AttachmentBase defaultAttachment)
            {
                int id = -1;
                if (m_Attachments.TryGetValue(slot, out var attachment) && attachment != null) {
                    id = attachment.Id;
                } else if(defaultAttachment != null) {
                    id = defaultAttachment.Id;
                }
                list.Add((slot, id));
            }

            f(IAttachment.AttachmentSlot.Scope, DefaultScope);
            f(IAttachment.AttachmentSlot.Magazine, DefaultMagazine);
            f(IAttachment.AttachmentSlot.Laser, DefaultLaser);
            f(IAttachment.AttachmentSlot.Grip, DefaultGrip);
            f(IAttachment.AttachmentSlot.Muzzle, DefaultMuzzle);

            return list;
        }
    }
}
