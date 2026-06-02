using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CrosshairData
{
    public Sprite Sprite;
    public Color Color;
    public float Scale;
}

[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour, IWeapon
{
    // Weapon
    GameObject m_Owner;
    public GameObject Owner => m_Owner;
    public Transform Muzzle;

    // Recoil
    [SerializeField] private float m_RecoilForce;
    [SerializeField] private float m_RecoilFrequency;
    [SerializeField] private float m_RecoilReturnSpeed;
    [SerializeField] private Vector3 m_SpineOffset;
    [SerializeField] Vector3 m_HandOffset;
    [SerializeField] Vector3 m_BackOffset;
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
    public Vector3 SpineOffset => m_SpineOffset;
    public Vector3 HandOffset => m_HandOffset;
    public Vector3 BackOffset => m_BackOffset;

    // Component
    IShootBehaviour m_Behaviour;
    IFireMechanism m_FireMechanism;
    Transform m_Target;
    AttachmentManager m_AttachmentManager;

    // Crosshair
    [SerializeField] CrosshairData m_Crosshair;
    public CrosshairData Crosshair => m_Crosshair;

    // Action
    public event Action OnFire;

    // Resrouce
    public AudioSource m_AudioSource;
    public GameObject MuzzleFlashPrefab;
    public AudioClip ShootSfx;
    public AudioClip ReloadingSfx;
    public AudioClip EmptyShootSfx;

    void Awake()
    {
        m_FireMechanism = GetComponent<IFireMechanism>();
        m_Behaviour = GetComponent<IShootBehaviour>();
        m_AmmoHandler = GetComponent<AmmoHandler>();
        m_AttachmentManager = GetComponentInChildren<AttachmentManager>();
        m_Behaviour.SetMuzzle(Muzzle);
        m_FireMechanism.OnShouldFire += TryFire;
    }

    void Update()
    {
        m_FireMechanism.UpdateFire(Time.deltaTime);
    }

    public void Initialize(GameObject holder)
    {
        m_Owner = holder;
        m_Behaviour.Initialize(this);
        m_AmmoHandler.Initialize(this);
    }

    public void StartFire(Transform target)
    {
        m_Target = target;
        m_FireMechanism.StartFire();
    }
    public void EndFire()
    {
        m_FireMechanism.StopFire();
        
        if (MuzzleFlashPrefab) {
            var sfx = Instantiate(MuzzleFlashPrefab, Muzzle.position, Muzzle.rotation, Muzzle);
            Destroy(sfx, 1f);
        }
        if (ShootSfx) {
            m_AudioSource.PlayOneShot(ShootSfx);
        }
    }

    void TryFire()
    {
        if(!m_AmmoHandler.ComsumeAmmo()) {
            return;
        }
        m_Behaviour.Shoot(Vector3.Normalize(m_Target.position - Muzzle.position));
        OnFire?.Invoke();
    }

    public bool ValidReload() => m_AmmoHandler.ValidReload();

    public void StartReload() => m_AmmoHandler.StartReload();
    public void EndReload(int ammo) => m_AmmoHandler.EndReload(ammo);

    public void ClearAmmo() => m_AmmoHandler.ComsumeAmmo(CurrentAmmo);
    public void SetParent(Transform parent) => transform.SetParent(parent, false);
    public void SetOffset(Vector3 offset) => transform.localPosition = offset;

    public void AddAttachment(AttachmentBase attachment) => m_AttachmentManager.AddAttachment(attachment);
    public void RemoveAttachment(AttachmentBase attachment) => m_AttachmentManager.RemoveAttachment(attachment);
    public Dictionary<IAttachment.AttachmentSlot, IAttachment> Attachments => m_AttachmentManager.Attachments;
    public float GetScopeRatio()
    {
        var scope = m_AttachmentManager.GetAttachment(IAttachment.AttachmentSlot.Scope);
        // £¿
        if(scope is Scope scope1) {
            return (float)scope1.Ratio;
        }
        return 1f;
    }
}
