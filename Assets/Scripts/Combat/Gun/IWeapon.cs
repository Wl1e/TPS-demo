using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public interface IWeapon
    {
        public GameObject GO { get; }
        public GameObject Owner { get; }
        public int WeaponId { get; }
        public float ReloadTime { get; }

        public float RecoilForce { get; }
        public float RecoilFrequency { get; }
        public float RecoilReturnSpeed { get; }

        public int AmmoId { get; }
        public int CurrentAmmo { get; }
        public int ClipAmmo { get; }
        public CrosshairData Crosshair { get; }

        public event Action OnFire;
        public event Action OnAttachmentChanged;

        public AttachableNode AttachNode { get; }

        public void Initialize(GameObject holder);
        public void StartFire(Transform target);
        public void EndFire();
        public bool ValidReload();
        public void StartReload();
        public void EndReload(int amount);
        public void ClearAmmo();
        public bool SupportAttachment(IAttachment.AttachmentSlot slot, int attachmentId);
        public void AddAttachment(IAttachment.AttachmentSlot slot, int attachmentId);
        public void RemoveAttachment(IAttachment.AttachmentSlot slot);
        public Dictionary<IAttachment.AttachmentSlot, IAttachment> Attachments { get; }
        public List<(IAttachment.AttachmentSlot, int)> GetAttachmentList();
        public float GetScopeRatio();
        public void OnEquip();
        public void OnUnequip();
        /// <summary>
        /// 必须由Server端调用
        /// </summary>
        /// <param name="node"></param>
        public void Attach(AttachableNode node);
        public void Detach();
    }
}
