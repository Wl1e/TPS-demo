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

    [RequireComponent(typeof(AudioSource))]
    public class Weapon: NetworkBehaviour, IWeapon
    {
        // 武器所有者
        GameObject m_Owner;
        public GameObject Owner => m_Owner;
        [Tooltip("枪口位置")]
        public Transform Muzzle;

        // Recoil
        [Header("Recoil")]
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
        AttachmentManager m_AttachmentManager;
        AttachableBehaviour m_Attachable;

        // Crosshair
        [SerializeField] CrosshairData m_Crosshair;
        public CrosshairData Crosshair => m_Crosshair;

        // Action
        public event Action OnFire;

        // Resrouce
        [Header("资源")]
        [Tooltip("枪口特效预制体")]
        public GameObject MuzzleFlashPrefab;
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
            }
        }

        public void Initialize(GameObject holder)
        {
            m_Owner = holder;
        }

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
            PlayAudioAndMuzzleFlash();
        }

        void PlayAudioAndMuzzleFlash()
        {
        if (MuzzleFlashPrefab) {
                // 枪口焰方向朝向-z，所以取反
                Director.Instance.RequestEffect(MuzzleFlashPrefab)
                    .WithParent(transform)
                    .WithPosition(Muzzle.localPosition)
                    .LookAt(-Muzzle.forward)
                    .WithDuration(m_FireMechanism.FireInternal)
                    .Create();
            }
            if (ShootSfx) {
                Director.Instance.RequestAudio(ShootSfx).AttachTo(transform).Play();
            }
        }

        void TryFire()
        {
            if (!m_AmmoHandler.EnoughAmmo()) {
                return;
            }
            FireServerRpc(Vector3.Normalize(m_Target.position - Muzzle.position));

            PlayAudioAndMuzzleFlash();
        }

        public bool ValidReload() => m_AmmoHandler.ValidReload();
        public void StartReload() => m_AmmoHandler.StartReload();
        public void EndReload(int ammo) => m_AmmoHandler.EndReload(ammo);

        public void ClearAmmo() => m_AmmoHandler.ComsumeAmmo(CurrentAmmo);

        public void AddAttachment(AttachmentBase attachment) => m_AttachmentManager.AddAttachment(attachment);
        public void RemoveAttachment(AttachmentBase attachment) => m_AttachmentManager.RemoveAttachment(attachment);
        public Dictionary<IAttachment.AttachmentSlot, IAttachment> Attachments => m_AttachmentManager.Attachments;
        public float GetScopeRatio()
        {
            var scope = m_AttachmentManager.GetAttachment(IAttachment.AttachmentSlot.Scope);
            // ？
            if (scope is Scope scope1) {
                return (float)scope1.Ratio;
            }
            return 1f;
        }

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

        public void Attach(AttachableNode node)
        {
            m_Attachable.Attach(node);
        }

        public NetworkObject GetNO() => GetComponentInParent<NetworkObject>();
    }
}
