using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    public interface IWeapon
    {
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

        public void Initialize(GameObject holder);
        public void StartFire(Transform target);
        public void EndFire();
        public bool ValidReload();
        public void StartReload();
        public void EndReload(int amount);
        public void ClearAmmo();
        public void SetOffset(Vector3 offset);
        public void AddAttachment(AttachmentBase attachment);
        public void RemoveAttachment(AttachmentBase attachment);
        public Dictionary<IAttachment.AttachmentSlot, IAttachment> Attachments { get; }
        public float GetScopeRatio();
        public void OnEquip();
        public void OnUnequip();
        public void Attach(AttachableNode node);
    }
}
