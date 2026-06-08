using System.Collections;
using Unity.VisualScripting;
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
        public bool IsClimb;
        public bool IsLedge;
        public bool IsMantle;
        public bool IsGrounded;
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
            Throw = false;
            Attack = other.Attack;
            WeaponType = other.WeaponType;
            IsClimb = other.IsClimb;
            IsLedge = other.IsLedge;
            IsMantle = other.IsMantle;
            CombatSlot = other.CombatSlot;
        }
    }

    public class AnimatorController : MonoBehaviour
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
            m_LastParameter.SmoothVelocity = smoothVel;

            SetBool("IsMove", curData.IsMove);
            SetFloat("HorizonalSpeed", smoothVel.x);
            SetFloat("VerticalSpeed", smoothVel.z);

            if (m_LastParameter.IsAim != curData.IsAim) {
                SetBool("Aim", curData.IsAim);
                //SetAimWeight(curData.IsAim);
            }

            if (m_LastParameter.IsSprint != curData.IsSprint) {
                SetBool("Sprint", curData.IsSprint);
            }
            if (curData.Reload) {
                SetTrigger("Reload");
            }
            if (m_LastParameter.Attack != curData.Attack) {
                SetBool("Attack", curData.Attack);
            }
            if (m_LastParameter.IsCrouch != curData.IsCrouch) {
                SetBool("Crouch", curData.IsCrouch);
            }
            if (m_LastParameter.WeaponType != curData.WeaponType) {
                SetInteger("WeaponType", curData.WeaponType);
            }

            if (curData.Jump) {
                SetTrigger("Jump");
                m_PlayerRuntimeData.AniParameter.Jump = false;
            }

            if (m_LastParameter.IsClimb != curData.IsClimb) {
                SetBool("IsClimb", curData.IsClimb);
            }
            if (m_LastParameter.IsLedge != curData.IsLedge) {
                SetBool("IsLedge", curData.IsLedge);
            }
            SetFloat("VelocityY", curData.Velocity.y);

            if (m_LastParameter.IsGrounded != curData.IsGrounded) {
                SetBool("Ground", curData.IsGrounded);
            }
            if(m_LastParameter.CombatSlot != curData.CombatSlot) {
                SetInteger("CombatSlot", curData.CombatSlot);
            }
            if (curData.Throw) {
                m_PlayerRuntimeData.AniParameter.Throw = false;
                SetTrigger("Throw");
            }

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
            } else if (m_PlayerRuntimeData.ActiveSlot == Combat.Slot.Throwable) {
            }
            m_RiggingCoroutine = StartCoroutine(AimRiggingCoroutine(targetAimWeight, targetSpineOffset));
        }
        //void SetAimWeight(float weight)
        //{
        //    Vector3 targetSpineOffset = GetFirearmSpineOffset();
        //    float startAimWeight = m_Rig.weight;
        //    Vector3 startSpineOffset = AimConstraint.data.offset;
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

        void SetAimLayerWeight(float layerWeight) => m_Animator.SetLayerWeight(1, layerWeight);

        public void ResetAnimation() => m_PlayerRuntimeData.AniParameter = new AnimatorParameter();
    }
}
