using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public struct AnimatorParameter
{
    public Vector3 Velocity;
    public bool IsMove;
    public bool IsSprint;
    public bool IsCrouch;
    public bool IsAim;
    public bool Reload;
    public bool Jump;
    public bool Attack;
    public bool IsClimb;
    public bool IsLedge;
    public bool IsMantle;
    public bool IsGrounded;
    public int WeaponType;
    public Vector3 SmoothVelocity;
    public AnimatorParameter(int i = 0)
    {
        Velocity = Vector3.zero;
        IsMove = false;
        IsSprint = false;
        IsCrouch = false;
        IsAim = false;
        Reload = false;
        Jump = false;
        Attack = false;
        IsClimb = false;
        IsLedge = false;
        IsMantle = false;
        IsGrounded = false;
        WeaponType = 0;
        SmoothVelocity = Vector3.zero;
    }

    public void Copy(AnimatorParameter other)
    {
        Velocity = other.Velocity;
        IsMove = other.IsMove;
        IsSprint = other.IsSprint;
        IsCrouch = other.IsCrouch;
        IsAim = other.IsAim;
        Reload = false;
        Jump = false;
        Attack = other.Attack;
        WeaponType = other.WeaponType;
        IsClimb= other.IsClimb;
        IsLedge = other.IsLedge;
        IsMantle = other.IsMantle;
    }
}

public class AnimatorController: MonoBehaviour
{
    [SerializeField] Transform RightHand;
    [SerializeField] Transform Back;
    [SerializeField] Animator m_Animator;

    public float DampTime = 0.1f;

    PlayerController m_PlayerController;
    PlayerRuntimeData m_PlayerRuntimeData;

    // Aim
    [SerializeField] Rig m_Rig;
    public MultiAimConstraint AimConstraint;
    public float RigLerpDuration = 0.15f;
    // 平滑修改weight，实现动画平滑移动
    Coroutine m_RiggingCoroutine;
    [SerializeField] Vector3 m_AimOffset = new Vector3(0, 40, 0);
    [SerializeField] Vector3 m_CrouchOffset = new Vector3(20, -10, 0);
    [SerializeField] Vector3 m_CrouchMoveOffset = new Vector3(35, -10, 0);

    AnimatorParameter m_LastParameter = new AnimatorParameter();

    readonly string IsMovePar = "IsMove";
    readonly string HorizonalSpeedPar = "HorizonalSpeed";
    readonly string VerticalSpeedPar = "VerticalSpeed";
    readonly string AimPar = "Aim";
    readonly string JumpPar = "Jump";
    readonly string SprintPar = "Sprint";
    readonly string ReloadPar = "Reload";
    readonly string AttackPar = "Attack";
    readonly string CrouchPar = "Crouch";
    readonly string WeaponTypePar = "WeaponType";

    private void Start()
    {
        m_PlayerController = GetComponent<PlayerController>();
        m_PlayerRuntimeData = m_PlayerController.RuntimeData;
    }

    private void OnDestroy()
    {
    }

    private void LateUpdate()
    {
        var curData = m_PlayerRuntimeData.AniParameter;

        Vector3 smoothVel = Vector3.Lerp(m_LastParameter.SmoothVelocity, curData.Velocity, 0.15f);
        SetBool(IsMovePar, curData.IsMove);
        SetFloat(HorizonalSpeedPar, smoothVel.x);
        SetFloat(VerticalSpeedPar, smoothVel.z);
        m_LastParameter.SmoothVelocity = smoothVel;

        if (m_LastParameter.IsAim != curData.IsAim) {
            SetBool(AimPar, curData.IsAim);
            SetAimWeight(curData.IsAim);
        }

        if (m_LastParameter.IsSprint != curData.IsSprint) {
            SetBool(SprintPar, curData.IsSprint);
        }
        if (curData.Reload) {
            SetTrigger(ReloadPar);
        }
        if (m_LastParameter.Attack != curData.Attack) {
            SetBool(AttackPar, curData.Attack);
        }
        if (m_LastParameter.IsCrouch != curData.IsCrouch) {
            SetBool(CrouchPar, curData.IsCrouch);
        }
        if(m_LastParameter.WeaponType != curData.WeaponType) {
            SetInteger(WeaponTypePar, curData.WeaponType);
        }

        print("curData.Jump" + curData.Jump);
        if(curData.Jump) {
            SetTrigger(JumpPar);
            m_PlayerRuntimeData.AniParameter.Jump = false;
        }

        if(m_LastParameter.IsClimb != curData.IsClimb) {
            SetBool("IsClimb", curData.IsClimb);
        }
        if(m_LastParameter.IsLedge != curData.IsLedge) {
            SetBool("IsLedge", curData.IsLedge);
        }
        SetFloat("VelocityY", curData.Velocity.y);

        //if (m_LastParameter.IsGrounded != curData.IsGrounded) {
            SetBool("Ground", curData.IsGrounded);
        //}

        m_LastParameter.Copy(curData);
    }

    void SetFloat(string name, float value) => m_Animator.SetFloat(name, value);
    void SetFloat(string name, float value, float dampTime, float deltaTime) => m_Animator.SetFloat(name, value, dampTime, deltaTime);
    public void SetBool(string name, bool value) => m_Animator.SetBool(name, value);
    public void SetTrigger(string name) => m_Animator.SetTrigger(name);
    void SetInteger(string name, int value) => m_Animator.SetInteger(name, value);
    public void SetSpineOffset(Vector3 offset)
    { }
    void SetAimWeight(bool IsAiming)
    {
        float targetAimWeight = IsAiming ? 1.0f : 0.0f;
        Vector3 targetSpineOffset = GetFirearmSpineOffset();
        if (m_RiggingCoroutine != null) {
            StopCoroutine(m_RiggingCoroutine);
            m_RiggingCoroutine = null;
        }
        if (m_PlayerRuntimeData.ActiveSlot == Combat.Slot.Firearm) {
            SetAimLayerWeight(IsAiming ? 1 : 0);
        } else if(m_PlayerRuntimeData.ActiveSlot == Combat.Slot.Throwable) {
        }
        m_RiggingCoroutine = StartCoroutine(AimRiggingCoroutine(targetAimWeight, targetSpineOffset));
    }

    Vector3 GetFirearmSpineOffset()
    {
        Vector3 offset = Vector3.zero;
        if (m_PlayerRuntimeData.IsAiming) {
            offset += m_AimOffset;
        }
        if (m_PlayerRuntimeData.State == PlayerMovementState.Crouch) {
            offset += m_PlayerController.Movement.IsMoved() ? m_CrouchMoveOffset : m_CrouchOffset;
        }
        return offset;
    }

    IEnumerator AimRiggingCoroutine(float targetAimWeight, Vector3 targetSpineOffset)
    {
        float startAimWeight = m_Rig.weight;
        Vector3 startSpineOffset = AimConstraint.data.offset;
        float elapsedTime = 0;

        while (elapsedTime < RigLerpDuration) {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / RigLerpDuration);
            m_Rig.weight = Mathf.Lerp(startAimWeight, targetAimWeight, t);
            AimConstraint.data.offset = Vector3.Lerp(startSpineOffset, targetSpineOffset, t);
            yield return null;
        }

        m_Rig.weight = targetAimWeight;
        AimConstraint.data.offset = targetSpineOffset;
        m_RiggingCoroutine = null;
    }

    void SetAimLayerWeight(float layerWeight)
    {
        m_Animator.SetLayerWeight(1, layerWeight);
    }

    public void ResetAnimation()
    {
        m_PlayerRuntimeData.AniParameter = new AnimatorParameter();
    }
}
