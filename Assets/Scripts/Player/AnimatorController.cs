using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace TPSDemo
{
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
        public bool IsGrounded;

        // climb
        public bool IsClimb;
        public bool IsLedge;
        public bool IsMantle;
        public bool DisableAimLayer;

        public int WeaponType;
        public Vector3 SmoothVelocity;
        public int CombatSlot;
        public bool Throw;
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
            CombatSlot = 0;
            Throw = false;
            DisableAimLayer = false;
        }

        public void Copy(AnimatorParameter other)
        {
            Velocity = other.Velocity;
            IsMove = other.IsMove;
            IsSprint = other.IsSprint;
            IsCrouch = other.IsCrouch;
            IsAim = other.IsAim;
            Reload = other.Reload;
            Jump = other.Jump;
            Throw = other.Throw;
            Attack = other.Attack;
            WeaponType = other.WeaponType;
            IsGrounded = other.IsGrounded;
            IsClimb = other.IsClimb;
            IsLedge = other.IsLedge;
            IsMantle = other.IsMantle;
            CombatSlot = other.CombatSlot;
            DisableAimLayer = other.DisableAimLayer;
        }
    }

    public class AnimatorController : NetworkBehaviour
    {
        [SerializeField] Animator m_Animator;
        [Tooltip("小于该值的速度为0（用于平滑动画）")]
        [SerializeField] float m_VelocityThreshold = 0.01f;
        Vector3 m_VelocityDampRef;

        public float DampTime = 0.1f;

        PlayerController m_PlayerController;
        PlayerRuntimeData m_PlayerRuntimeData;

        // Aim
        [SerializeField] Rig m_Rig;
        public MultiAimConstraint AimConstraint;
        public float RigLerpDuration = 0.15f;
        // 平滑修改weight，实现动画平滑移动
        Coroutine m_RiggingCoroutine;
        [SerializeField] Vector3 m_CrouchOffset = new Vector3(20, -10, 0);
        [SerializeField] Vector3 m_CrouchMoveOffset = new Vector3(35, -10, 0);

        AnimatorParameter m_LastParameter = new AnimatorParameter();

        private void Start()
        {
            m_PlayerController = GetComponent<PlayerController>();
            m_PlayerRuntimeData = m_PlayerController.RuntimeData;
            m_Rig.weight = 0f;
        }

        private void LateUpdate()
        {
            if(!IsOwner) {
                return;
            }
            var curData = m_PlayerRuntimeData.AniParameter;

            //Vector3 smoothVel = Vector3.Lerp(m_LastParameter.SmoothVelocity, curData.Velocity, 0.15f);
            Vector3 smoothVel = Vector3.SmoothDamp(
                m_LastParameter.SmoothVelocity,
                curData.Velocity,
                ref m_VelocityDampRef,
                0.15f
            );

            if (Mathf.Abs(smoothVel.x) <= m_VelocityThreshold) {
                smoothVel.x = 0f;
            }
            if (Mathf.Abs(smoothVel.z) <= m_VelocityThreshold) {
                smoothVel.z = 0f;
            }

            m_LastParameter.SmoothVelocity = smoothVel;

            // property
            SetBool("IsMove", curData.IsMove);
            SetFloat("HorizonalSpeed", smoothVel.x);
            SetFloat("VerticalSpeed", smoothVel.z);
            SetFloat("VelocityY", curData.Velocity.y);
            if (m_LastParameter.IsGrounded != curData.IsGrounded) {
                SetBool("Ground", curData.IsGrounded);
            }

            // move
            if (m_LastParameter.IsSprint != curData.IsSprint) {
                SetBool("Sprint", curData.IsSprint);
            }
            if (m_LastParameter.Reload != curData.Reload) {
                UpdateTrigger("Reload", curData.Reload);
            }
            if (m_LastParameter.Attack != curData.Attack) {
                SetBool("Attack", curData.Attack);
            }
            if (m_LastParameter.IsCrouch != curData.IsCrouch) {
                SetBool("Crouch", curData.IsCrouch);
            }
            if (m_LastParameter.Jump != curData.Jump) {
                UpdateTrigger("Jump", curData.Jump);
            }

            if (m_LastParameter.WeaponType != curData.WeaponType) {
                SetInteger("WeaponType", curData.WeaponType);
            }

            // climb
            if (m_LastParameter.IsMantle != curData.IsMantle) {
                print("UpdateMantle");
                UpdateTrigger("Mantle", curData.IsMantle);
            }
            if (m_LastParameter.IsClimb != curData.IsClimb) {
                SetBool("Climb", curData.IsClimb);
            }
            if (m_LastParameter.IsLedge != curData.IsLedge) {
                SetBool("Ledge", curData.IsLedge);
            }
            if(m_LastParameter.DisableAimLayer != curData.DisableAimLayer) {
                SetAimLayerWeight(curData.DisableAimLayer ? 0f : 1f);
            }

            // combat
            if(m_LastParameter.CombatSlot != curData.CombatSlot) {
                SetInteger("CombatSlot", curData.CombatSlot);
            }
            if (m_LastParameter.Throw != curData.Throw) {
                UpdateTrigger("Throw", curData.Throw);
            }
            if (m_LastParameter.IsAim != curData.IsAim) {
                SetBool("Aim", curData.IsAim);
                //SetAimWeight(curData.IsAim);
            }

            m_LastParameter.Copy(curData);
        }

        void SetFloat(string name, float value) => m_Animator.SetFloat(name, value);
        void SetFloat(string name, float value, float dampTime, float deltaTime) => m_Animator.SetFloat(name, value, dampTime, deltaTime);
        void SetBool(string name, bool value) => m_Animator.SetBool(name, value);
        void SetTrigger(string name) => m_Animator.SetTrigger(name);
        void ResetTrigger(string name) => m_Animator.ResetTrigger(name);
        void UpdateTrigger(string name, bool value)
        {
            if (value) {
                SetTrigger(name);
            } else {
                ResetTrigger(name);
            }
        }
        void SetInteger(string name, int value) => m_Animator.SetInteger(name, value);
        public void SetSpineOffset(Vector3 offset)
        { }

        public void SetAimWeight(bool IsAiming)
        {
            float targetAimWeight = IsAiming ? 1.0f : 0.0f;
            Vector3 targetSpineOffset = AimConstraint.data.offset + GetFirearmSpineOffset();
            if (m_RiggingCoroutine != null) {
                StopCoroutine(m_RiggingCoroutine);
                m_RiggingCoroutine = null;
            }
            m_RiggingCoroutine = StartCoroutine(AimRiggingCoroutine(targetAimWeight, targetSpineOffset));
        }

        //public void SetAimWeight(float weight)
        //{
        //    float startAimWeight = m_Rig.weight;
        //    Vector3 startSpineOffset = AimConstraint.data.offset;
        //    Vector3 targetSpineOffset = startSpineOffset + GetFirearmSpineOffset();
        //    float elapsedTime = 0;

        //    while (elapsedTime < RigLerpDuration) {
        //        elapsedTime += Time.deltaTime;
        //        float t = Mathf.Clamp01(elapsedTime / RigLerpDuration);
        //        m_Rig.weight = Mathf.Lerp(startAimWeight, weight, t);
        //        AimConstraint.data.offset = Vector3.Lerp(startSpineOffset, targetSpineOffset, t);
        //    }

        //    m_Rig.weight = weight;
        //    AimConstraint.data.offset = targetSpineOffset;
        //    m_RiggingCoroutine = null;
        //}

        Vector3 GetFirearmSpineOffset()
        {
            Vector3 offset = Vector3.zero;
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

        void SetAimLayerWeight(float layerWeight) => m_Animator.SetLayerWeight(1, layerWeight);

        public void ResetAnimation() => m_PlayerRuntimeData.AniParameter = new AnimatorParameter();
    }
}
