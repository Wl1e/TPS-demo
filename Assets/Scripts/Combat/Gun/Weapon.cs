using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace TPSDemo
{

    [Serializable]
    public struct CrosshairData
    {
        public Sprite Sprite;
        public Color Color;
        public float Scale;
    }

    public class Weapon: NetworkBehaviour, IWeapon
    {
        GameObject m_Owner;

        [SerializeField] private WeaponItemData m_Config;
        public WeaponItemData Config => m_Config;

        #region Property

        public GameObject GO => gameObject ?? null;
        public GameObject Owner => m_Owner;

        public int WeaponId => m_Config.Id;
        public float ReloadTime => m_AmmoHandler.ReloadTime;
        public float RecoilFrequency => m_Config.RecoilFrequency;
        public float RecoilForce => m_Config.RecoilForce;
        public float RecoilReturnSpeed => m_Config.RecoilReturnSpeed;

        public int CurrentAmmo => m_AmmoHandler.CurrentAmmo;
        public int ClipAmmo => m_Config.DefaultClipSize;
        public int AmmoId => m_Config.AmmoId;

        public CrosshairData Crosshair => m_Config.Crosshair;

        #endregion

        [Tooltip("枪口")]
        public Transform Muzzle;

        private Unity.Cinemachine.CinemachineImpulseSource m_ImpulseSource;

        // Ammo
        AmmoHandler m_AmmoHandler;

        // Offset
        //public Vector3 SpineOffset => m_SpineOffset;
        //public Vector3 HandOffset => m_HandOffset;
        //public Vector3 BackOffset => m_BackOffset;

        // Component
        /// <summary>
        /// </summary>
        IShootBehaviour m_Behaviour;
        /// <summary>
        /// </summary>
        IFireMechanism m_FireMechanism;
        Transform m_Target;
        /// <summary>
        /// </summary>
        AttachmentManager m_AttachmentManager;

        AttachableBehaviour m_Attachable;

        /// <summary>
        /// 
        /// </summary>
        private PlayableController m_Animator;
        
        // Action
        public event Action OnFire;
        public event Action OnAttachmentChanged;

        [Tooltip("抛壳位置")]
        public Transform ShellEjectPoint;

        void Awake()
        {
            m_FireMechanism = GetComponent<IFireMechanism>();
            m_Behaviour = GetComponent<IShootBehaviour>();
            m_AmmoHandler = GetComponent<AmmoHandler>();
            m_AttachmentManager = GetComponentInChildren<AttachmentManager>();
            m_Attachable = GetComponent<AttachableBehaviour>();
            m_Behaviour.SetMuzzle(Muzzle);
            m_Animator = GetComponentInChildren<PlayableController>();
            m_ImpulseSource = GetComponent<Unity.Cinemachine.CinemachineImpulseSource>();
        }

        void Update()
        {
            m_FireMechanism.UpdateFire(Time.deltaTime);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if(IsOwner) {
                m_FireMechanism.OnShouldFire += TryFire;
                m_AttachmentManager.OnAttachmentChanged += AttachmentChanged;
                m_AttachmentManager.Weapon = this;
            }
        }

        public void Initialize(GameObject holder)
        {
            m_Owner = holder;
            m_AmmoHandler.Initialize(this);
            m_Behaviour.Initialize(this);
            m_FireMechanism.Initialize(this);
        }

        #region Fire

        public void StartFire(Transform target)
        {
            m_Target = target;
            m_FireMechanism.StartFire();
        }
        public void EndFire()
        {
            m_FireMechanism.StopFire();
        }

        /// <summary>
        /// </summary>
        [ServerRpc]
        void FireServerRpc(Vector3 position)
        {
            if (!m_AmmoHandler.ComsumeAmmo()) {
                return;
            }
            m_Behaviour.Shoot(position - Muzzle.position, OwnerClientId);
            FireClientRpc();
        }

        /// <summary>
        /// </summary>
        [ClientRpc]
        void FireClientRpc()
        {
            if(IsOwner) {
                OnFire?.Invoke();
                return;
            }

            PlayerClientEffects();
        }

        void TryFire()
        {
            if (!m_AmmoHandler.EnoughAmmo()) {
                return;
            }
            FireServerRpc(m_Target.position);

            PlayerClientEffects();
            if (m_ImpulseSource) {
                m_ImpulseSource.GenerateImpulse();
            }
        }

        private void PlayerClientEffects()
        {
            if (m_Animator) {
                m_Animator.Play("Fire");
            }
            if (m_Config.ShellPrefab && ShellEjectPoint) {
                var shell = Instantiate(m_Config.ShellPrefab, ShellEjectPoint.position, ShellEjectPoint.rotation, null);
                if (shell.TryGetComponent<Rigidbody>(out var rb)) {
                    rb.AddForce(ShellEjectPoint.forward * m_Config.ShellEjectForce, ForceMode.Impulse);
                    Destroy(shell, 3f);
                }
            }

            Director.Instance.RequestEffect(m_Config.MuzzleFlashPrefab)
                .WithParent(Muzzle)
                .LookAt(-Muzzle.forward)
                .WithDuration(m_Config.MuzzleFlashTime)
                .Create();

            Director.Instance.RequestAudio(m_Config.ShootSfx)
                .WithMixerGroup(AudioSystem.AudioGroup.SFX)
                .AttachTo(transform)
                .Play();
        }

        #endregion

        #region Ammo

        public bool ValidReload() => m_AmmoHandler.ValidReload();
        public void StartReload() => m_AmmoHandler.StartReload();
        public void EndReload(int ammo) => m_AmmoHandler.EndReload(ammo);

        // 提前设置好子弹数量
        public void SetAmmo(int amount) => m_AmmoHandler.SetAmmo(amount);
        public void ClearAmmo() => m_AmmoHandler.ComsumeAmmo(CurrentAmmo);

        #endregion

        #region Attachment

        public bool SupportAttachment(IAttachment.AttachmentSlot slot, int attachmentId) => m_AttachmentManager.SupportAttachment(slot, attachmentId);
        public void AddAttachment(IAttachment.AttachmentSlot slot, int attachmentId) => m_AttachmentManager.AddAttachment(slot, attachmentId);
        public void RemoveAttachment(IAttachment.AttachmentSlot slot) => m_AttachmentManager.RemoveAttachment(slot);
        public Dictionary<IAttachment.AttachmentSlot, IAttachment> Attachments => m_AttachmentManager.Attachments;

        public float GetScopeRatio()
        {
            var scope = m_AttachmentManager.GetAttachment(IAttachment.AttachmentSlot.Scope);
            // ��
            if (scope != null && scope is ScopeAttachment scope1) {
                return (float)scope1.Ratio;
            }
            return 1f;
        }

        public List<(IAttachment.AttachmentSlot, int)> GetAttachmentList() => m_AttachmentManager.GetAttachmentList();

        private void AttachmentChanged()
        {
            OnAttachmentChanged?.Invoke();
        }

        #endregion

        #region Equip

        public void OnEquip()
        {
            // Server���ã���������������ɸĳ�Owner���ã�Rpc�޸������ͻ���ƫ��
            //transform.SetLocalPositionAndRotation(m_HandOffset, Quaternion.Euler(m_HandRotation));
            //OnEquipClientRpc();
        }

        [ClientRpc]
        private void OnEquipClientRpc()
        {
            //transform.SetLocalPositionAndRotation(m_HandOffset, Quaternion.Euler(m_HandRotation));
        }

        public void OnUnequip()
        {
            //transform.SetLocalPositionAndRotation(m_BackOffset, Quaternion.Euler(Vector3.zero));
            //transform.localPosition = m_BackOffset;
            //transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        private AttachableNode m_AttachableNode;
        public AttachableNode AttachNode => m_AttachableNode;

        public void Attach(AttachableNode node)
        {
            m_AttachableNode = node;
            m_Attachable.Attach(m_AttachableNode);
        }

        public void Detach() => m_Attachable.Detach();

        #endregion
    }
}
