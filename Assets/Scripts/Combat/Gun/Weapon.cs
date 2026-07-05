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
        // 武器所有者
        GameObject m_Owner;
        public GameObject GO => gameObject;
        public GameObject Owner => m_Owner;
        [Tooltip("枪口位置")]
        public Transform Muzzle;

        // Recoil
        [Header("后坐力")]
        [Tooltip("后坐力大小")]
        [SerializeField] private float m_RecoilForce;
        [Tooltip("后坐力频率")]
        [SerializeField] private float m_RecoilFrequency;
        [Tooltip("后坐力回弹速度")]
        [SerializeField] private float m_RecoilReturnSpeed;

        [Header("偏移")]
        [Tooltip("脊柱偏移")]
        [SerializeField] private Vector3 m_SpineOffset;
        [Tooltip("手部偏移")]
        [SerializeField] Vector3 m_HandOffset;
        [Tooltip("手部旋转")]
        [SerializeField] Vector3 m_HandRotation;
        [Tooltip("背部偏移")]
        [SerializeField] Vector3 m_BackOffset;
        [Tooltip("武器ID")]
        [SerializeField] int m_WeaponId;
        public int WeaponId => m_WeaponId;
        public float ReloadTime => m_AmmoHandler.ReloadTime;
        public float RecoilFrequency => m_RecoilFrequency;
        public float RecoilForce => m_RecoilForce;
        public float RecoilReturnSpeed => m_RecoilReturnSpeed;

        // Ammo
        AmmoHandler m_AmmoHandler;
        public int CurrentAmmo => m_AmmoHandler.CurrentAmmo;
        public int ClipAmmo => m_AmmoHandler.ClipSize;
        public int AmmoId => m_AmmoHandler.AmmoId;

        // Offset
        //public Vector3 SpineOffset => m_SpineOffset;
        //public Vector3 HandOffset => m_HandOffset;
        //public Vector3 BackOffset => m_BackOffset;

        // Component
        /// <summary>
        /// 发射行为（一颗子弹、霰弹枪范围随机）
        /// </summary>
        IShootBehaviour m_Behaviour;
        /// <summary>
        /// 发射规律（自动、半自动、三连发）
        /// </summary>
        IFireMechanism m_FireMechanism;
        Transform m_Target;
        /// <summary>
        /// 配件管理
        /// </summary>
        AttachmentManager m_AttachmentManager;

        AttachableBehaviour m_Attachable;

        /// <summary>
        /// 
        /// </summary>
        private ManualAnimatorController m_Animator;

        [Tooltip("准星")]
        [SerializeField] CrosshairData m_Crosshair;
        public CrosshairData Crosshair => m_Crosshair;

        // Action
        public event Action OnFire;
        public event Action OnAttachmentChanged;

        [Tooltip("抛弹壳")]
        public Transform ShellEjectPoint;
        [Tooltip("抛弹力度")]
        public float ShellEjectForce = 1f;
        [Tooltip("抛壳特效预制体")]
        public UnityEngine.AddressableAssets.AssetReference ShellPrefab;

        // Resrouce
        [Header("资源")]
        [Tooltip("枪口特效预制体")]
        public GameObject MuzzleFlashPrefab;
        [Tooltip("枪焰持续时间")]
        public float MuzzleFlashTime = 0.09f;
        [Tooltip("射击音效")]
        public AudioClip ShootSfx;
        [Tooltip("换弹音效")]
        public AudioClip ReloadingSfx;

        void Awake()
        {
            m_FireMechanism = GetComponent<IFireMechanism>();
            m_Behaviour = GetComponent<IShootBehaviour>();
            m_AmmoHandler = GetComponent<AmmoHandler>();
            m_AttachmentManager = GetComponentInChildren<AttachmentManager>();
            m_Attachable = GetComponent<AttachableBehaviour>();
            m_Behaviour.SetMuzzle(Muzzle);
            m_Animator = GetComponentInChildren<ManualAnimatorController>();
        }

        void Update()
        {
            m_FireMechanism.UpdateFire(Time.deltaTime);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if(IsServer || IsOwner) {
                m_AmmoHandler.Initialize(this);
                m_Behaviour.Initialize(this);
            }
            if(IsOwner) {
                m_FireMechanism.OnShouldFire += TryFire;
                m_AttachmentManager.OnAttachmentChanged += AttachmentChanged;
                m_AttachmentManager.Weapon = this;
            }
        }

        public void Initialize(GameObject holder)
        {
            m_Owner = holder;
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
        /// Server端 发射逻辑（扣子弹、同时其他Client副本播放特效）
        /// </summary>
        [ServerRpc]
        void FireServerRpc(Vector3 dir)
        {
            if (!m_AmmoHandler.ComsumeAmmo()) {
                return;
            }
            m_Behaviour.Shoot(dir, OwnerClientId);
            FireClientRpc();
        }

        /// <summary>
        /// Client端 发射逻辑（只播放音效和动画）
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
            FireServerRpc(m_Target.position - Muzzle.position);

            PlayerClientEffects();
        }

        // 在自身和各个客户端播放(动画、抛出弹壳、枪焰、枪声)
        // 为什么不用AudioAndEffectPlayGlobal?
        // 开火需要Server判断(FireServerRpc)，然后才能通过rpc返回到owner执行开火，那么
        // 干脆让这个rpc直接让所有client播放效果，免得owner向其他client再发rpc
        private void PlayerClientEffects()
        {
            if (m_Animator) {
                m_Animator.Play("Fire");
            }
            if (ShellPrefab.RuntimeKeyIsValid() && ShellEjectPoint) {
                StartCoroutine(AssetCache.GetOrLoad(ShellPrefab, ShellEjectPoint.position, ShellEjectPoint.rotation, null, obj => {
                    if (obj.TryGetComponent<Rigidbody>(out var rb)) {
                        rb.AddForce(ShellEjectPoint.forward * ShellEjectForce, ForceMode.Impulse);
                        Destroy(obj, 3f);
                    }
                }));
            }

            // 枪口焰方向朝向-z，所以取反
            Director.Instance.RequestEffect(MuzzleFlashPrefab)
                .WithParent(Muzzle)
                .LookAt(-Muzzle.forward)
                .WithDuration(MuzzleFlashTime)
                .Create();

            Director.Instance.RequestAudio(ShootSfx).AttachTo(transform).Play();
        }

        #endregion

        #region Ammo

        public bool ValidReload() => m_AmmoHandler.ValidReload();
        public void StartReload() => m_AmmoHandler.StartReload();
        public void EndReload(int ammo) => m_AmmoHandler.EndReload(ammo);

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
            // ？
            if (scope is ScopeAttachment scope1) {
                return (float)scope1.Ratio;
            }
            return 1f;
        }

        public List<(IAttachment.AttachmentSlot, int)> GetAttachmentList() => m_AttachmentManager.GetAttachmentList();

        #endregion

        #region Pos

        public void OnEquip()
        {
            transform.localPosition = m_HandOffset;
            transform.localRotation = Quaternion.Euler(m_HandRotation);
        }

        public void OnUnequip()
        {
            transform.localPosition = m_BackOffset;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
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


        private void AttachmentChanged()
        {
            print("Weapon.AttachmentChanged");
            OnAttachmentChanged?.Invoke();
        }
    }
}
