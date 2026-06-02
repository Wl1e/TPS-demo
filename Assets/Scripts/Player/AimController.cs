using Event;
using System;
using System.Collections;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(WeaponManager))]
public class AimController : MonoBehaviour
{
    // Camera
    public Transform AimTransform;
    public float SmoothingSpeed = 20f;
    Camera m_Camera;

    // AimPoint
    public Transform VisualAimPointTransform;
    public LayerMask AimRayCastLayerMask = ~0;
    public float AimRayDistance = 200f;
    Vector3 m_VisualAimPosition;

    // Events
    public BoolEvent OnTryAimEvent;
    public BoolEvent OnFireEvent;

    // Weapon
    CombatController m_CombatController;
    WeaponManager m_WeaponManager;
    public float RecoilForce => m_WeaponManager.CurrentFirearm?.RecoilForce ?? 0;
    public float RecoilKickSpeed = 50f;
    public float RecoilReturnSpeed => m_WeaponManager.CurrentFirearm?.RecoilReturnSpeed ?? 0;
    Vector3 m_CurrentRecoilOffset;
    Vector3 m_TargetRecoilOffset;

    public bool IsAiming { get; private set; } = false;

    // 修正蹲下时的动画骨骼位置和角度
    PlayerController m_PlayerController;
    PlayerRuntimeData m_RuntimeData;

    void Start()
    {
        m_PlayerController = GetComponentInParent<PlayerController>();
        m_RuntimeData = m_PlayerController.RuntimeData;
        m_CombatController = GetComponent<CombatController>();
        m_WeaponManager = GetComponent<WeaponManager>();

        m_Camera = Camera.main;
    }

    private void OnEnable()
    {
        OnFireEvent.RegisterListener(HandleWeaponFired);
        OnTryAimEvent.RegisterListener(HandleTryAim);
        EventManager.AddListener<WeaponChangedEvent>(HandleChangeWeapon);
        EventManager.AddListener<WeaponFiredEvent>(OnFired);
    }

    private void OnDisable()
    {
        OnFireEvent.UnregisterListener(HandleWeaponFired);
        OnTryAimEvent.UnregisterListener(HandleTryAim);
        EventManager.RemoveListener<WeaponChangedEvent>(HandleChangeWeapon);
        EventManager.RemoveListener<WeaponFiredEvent>(OnFired);
    }

    void Update()
    {
        if(IsAiming) {
            UpdateAimPositon();
        } else {

        }
    }

    private void LateUpdate()
    {
        if (IsAiming) {
            m_CurrentRecoilOffset = Vector3.Lerp(m_CurrentRecoilOffset, m_TargetRecoilOffset, RecoilKickSpeed * Time.deltaTime);
            m_TargetRecoilOffset = Vector3.Lerp(m_TargetRecoilOffset, Vector3.zero, RecoilReturnSpeed * Time.deltaTime);
            VisualAimPointTransform.position = m_VisualAimPosition + m_CurrentRecoilOffset;
            VisualAimPointTransform.gameObject.SetActive(true);
        } else {
            VisualAimPointTransform.gameObject.SetActive(false);
        }
    }

    void UpdateAimPositon()
    {
        if(!m_Camera) {
            // 玩家还没生成（相机未就绪）或相机被销毁时，确保网络同步有值可用，而不是零向量
            return;
        }

        Ray ray = m_Camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        Vector3 targetPosition;
        float distance = 0f;
        if(Physics.Raycast(ray, out RaycastHit hit, AimRayDistance, AimRayCastLayerMask)) {
            targetPosition = hit.point;
            distance = hit.distance;
        } else {
            targetPosition = ray.origin + ray.direction * AimRayDistance;
            distance = AimRayDistance;
        }
        // TODO 如果距离过近，会有IK动画问题，引入visualAimBlendDistance
        m_VisualAimPosition = targetPosition;
    }

    bool ValidAim()
    {
        return m_CombatController.ValidAim();
    }

    void HandleTryAim(bool tryAim)
    {
        if (tryAim && !ValidAim()) {
            return;
        } else if(!tryAim && !IsAiming) {
            return;
        }
        IsAiming = tryAim;
        if (!IsAiming) {
            m_CurrentRecoilOffset = Vector3.zero;
            m_TargetRecoilOffset = Vector3.zero;
        }
        m_RuntimeData.IsAiming = IsAiming;
        m_RuntimeData.AniParameter.IsAim = IsAiming;
        EventManager.Broadcast(new AimEvent { IsAiming = IsAiming });
    }

    void HandleWeaponFired(bool fire)
    {
    }
    void HandleChangeWeapon(WeaponChangedEvent evt)
    {

    }

    void ApplyRecoil()
    {
        IWeapon weapon = m_WeaponManager.CurrentFirearm;
        if(weapon is null) {
            return;
        }
        float Frequency = weapon.RecoilFrequency;
        float Force = weapon.RecoilForce;
        Vector3 recoil = new Vector3(
            Mathf.Sin(Time.time * Frequency) * Force,
            (Mathf.Sin(Time.time * Frequency * 2f) * 0.5f + 0.5f) * Force,
            0
        );
        m_TargetRecoilOffset += recoil;
    }

    void OnFired(WeaponFiredEvent evt)
    {
        ApplyRecoil();
    }
}
