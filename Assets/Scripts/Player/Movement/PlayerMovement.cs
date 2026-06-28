using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace TPSDemo
{

    /// <summary>
    /// 玩家移动
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : NetworkTransform
    {
#if UNITY_EDITOR
        // Inspector view expand/collapse settings for this derived child class
        [HideInInspector]
        public bool MoverScriptExpanded;
#endif
        /// <summary>
        /// 身体旋转和镜头移动是否绑定
        /// </summary>
        public enum CouplingMode
        {
            Decoupled,
            Coupled
        }

        /// <summary>
        /// 移动锁
        /// </summary>
        public CountDownLatch MovementLock = new();

        CharacterController m_CharacterController;
        PlayerRuntimeData m_PlayerRuntimeData;
        PlayerStateMachine m_PlayerStateMachine;

        /// <summary>
        /// 移动能力
        /// </summary>
        Dictionary<PlayerMovementState, IMovementAbility> m_AbilitiesLookup;

        /// 各种移动的速度
        /// 移动全权交给代码实现，不依赖RootMotion
        /// 本来是打算烘培动作速度曲线来实现不滑步的，但是八向移动使用的混合树
        /// 遂放弃，改为固定速度，后续可以考虑通过速度和动画速度的比值来调整动画播放速度来实现不滑步
        [Tooltip("行走速度")]
        [SerializeField] float m_WalkSpeed = 2f;

        // Constant
        [Tooltip("加速度")]
        public float Acceleration = 10f;
        [Tooltip("空中加速度")]
        public float AirAcceleration = 5f;
        [Tooltip("重力")]
        public float Gravity = 9.81f;

        public LayerMask GroundLayer = -1;

        [Tooltip("跳跃力")]
        public float JumpForce = 5f;

        // Rotation
        [Tooltip("旋转时间")]
        public float RotationSmoothTime = 0.1f;
        float m_RotationVelocity;
        public CouplingMode RotationType;

        Vector2 m_RawInput;
        Vector3 m_Input;
        Vector3 m_InputGlobal;
        /// <summary>
        /// 原始输入Vector2
        /// </summary>
        public Vector2 RawInput => m_RawInput;
        /// <summary>
        /// 原始输入Vector3
        /// </summary>
        public Vector3 Input => m_Input;
        /// <summary>
        /// 世界空间下输入
        /// </summary>
        public Vector3 InputGlobal => m_InputGlobal;
        public PlayerMovementState CurState = PlayerMovementState.Idle;

        /// <summary>
        /// 玩家速度
        /// </summary>
        public Vector3 Velocity => m_Velocity;

        Vector3 m_Velocity;

        /// <summary>
        /// 是否在地面
        /// </summary>
        public bool IsGrounded => m_IsGrounded;
        bool m_IsGrounded;
        bool m_JumpThisFrame;

        // Event
        [SerializeField] Vector2Event OnMoveInput;

        protected override void Awake()
        {
            base.Awake();

            var player = GetComponent<PlayerController>();
            m_CharacterController = player.CharacterController;
            m_PlayerRuntimeData = player.RuntimeData;
            m_PlayerStateMachine = player.StateMachine;


            m_AbilitiesLookup = new Dictionary<PlayerMovementState, IMovementAbility>();
            foreach (var ability in GetComponentsInChildren<IMovementAbility>()) {
                ability.Initialize(this);
                m_AbilitiesLookup.Add(ability.State, ability);
            }

            m_PlayerStateMachine.OnStateChanged += OnStateChanged;
            OnMoveInput.RegisterListener(OnMove);
            EventManager.AddListener<Event.AimEvent>(OnAim);
        }

        public override void OnDestroy()
        {
            m_PlayerStateMachine.OnStateChanged -= OnStateChanged;
            OnMoveInput.UnregisterListener(OnMove);
            EventManager.RemoveListener<Event.AimEvent>(OnAim);

            base.OnDestroy();
        }

        void Update()
        {
            if (!IsOwner) {
                return;
            }

            if(!m_CharacterController.enabled) {
                m_Velocity = Vector3.zero;
                return;
            }

            m_InputGlobal = Vector3.Normalize(Quaternion.Euler(0, m_PlayerRuntimeData.CameraRoot.eulerAngles.y, 0) * m_Input);

            GroundCheck();
            UpdateMovement();
            if (CurState != PlayerMovementState.Climb && CurState != PlayerMovementState.Ledge) {
                RotationBody();
            }
            m_CharacterController.Move(m_Velocity * Time.deltaTime);
            m_PlayerRuntimeData.AniParameter.IsMove = (m_Input != Vector3.zero);
            m_PlayerRuntimeData.AniParameter.Velocity = transform.InverseTransformDirection(m_Velocity);
            m_PlayerRuntimeData.AniParameter.IsGrounded = m_IsGrounded;

            //print($"InputVector: {m_InputGlobal}, Velocity: {m_Velocity}, selfVelocity: {transform.InverseTransformDirection(m_Velocity)}");
        }

        void UpdateMovement()
        {
            var velocityXZ = Vector3.ProjectOnPlane(m_Velocity, Vector3.up);
            float velocityY = m_Velocity.y;
            if (MovementLock.IsLockd()) {
                if (m_IsGrounded) {
                    if (velocityY != 0) {
                        velocityY = 0;
                    }
                } else if (!m_IsGrounded) {
                    velocityY -= Gravity * Time.deltaTime;
                }
                m_Velocity = Vector3.MoveTowards(velocityXZ, Vector3.zero, Acceleration * Time.deltaTime) + Vector3.up * m_Velocity.y;
                return;
            }
            var abilityModifier = GetAbilityModifier();
            if (abilityModifier != null) {
                Vector3 targetVelocity = abilityModifier.Velocity;
                m_Velocity = targetVelocity;
                if (abilityModifier.OverrideRotation) {
                    transform.rotation = Quaternion.Euler(abilityModifier.Euler);
                }
                return;
            }


            if (m_IsGrounded) {
                // 落地清空y轴速度
                if (velocityY != 0) {
                    velocityY = 0;
                }
                // 跳跃
                if (m_JumpThisFrame) {
                    velocityY += JumpForce;
                    m_JumpThisFrame = false;
                } else {
                    if (m_Input != Vector3.zero) {
                        velocityXZ += m_InputGlobal * Acceleration * Time.deltaTime;
                        if (CurState == PlayerMovementState.Walk) {
                            velocityXZ = Vector3.ClampMagnitude(velocityXZ, m_WalkSpeed);
                        }
                    } else {
                        velocityXZ = Vector3.MoveTowards(velocityXZ, Vector3.zero, Acceleration * Time.deltaTime);
                    }
                }
            } else {
                velocityY -= Gravity * Time.deltaTime;
                if (m_Input != Vector3.zero) {
                    var velocitySpeed = velocityXZ.magnitude;
                    velocityXZ += m_InputGlobal * AirAcceleration * Time.deltaTime;
                    velocityXZ = Vector3.ClampMagnitude(velocityXZ, velocitySpeed);
                } else {
                    velocityXZ = Vector3.MoveTowards(velocityXZ, Vector3.zero, AirAcceleration * Time.deltaTime);
                }
            }

            m_Velocity = velocityXZ + Vector3.up * velocityY;

            //print($"InputVector: {m_InputGlobal}, Velocity: {m_Velocity}, TargetData: {m_TargetData.name}");
        }

        MovementModifier GetAbilityModifier()
        {
            if (m_AbilitiesLookup.TryGetValue(m_PlayerRuntimeData.State, out var ability)) {
                return ability.Process();
            }
            return null;
        }

        void OnStateChanged(PlayerMovementState state)
        {
            if (state == PlayerMovementState.Jump && m_IsGrounded) {
                m_JumpThisFrame = true;
            } else {
                UpdateTargetSpeed(state);
            }
        }

        void UpdateTargetSpeed(PlayerMovementState state)
        {
            if (CurState == PlayerMovementState.Climb && state == PlayerMovementState.Jump) {
                m_Velocity = transform.forward * -m_WalkSpeed + Vector3.up * JumpForce;
            }
            CurState = state;
            if (state == PlayerMovementState.Walk) {
            } else if (state == PlayerMovementState.Idle) {
            } else if (state == PlayerMovementState.Jump) {
            } else if (state == PlayerMovementState.Sprint) {
            } else if (state == PlayerMovementState.Crouch) {
            }
            //print($"UpdateData: state{state}, IsAiming: {m_PlayerRuntimeData.IsAiming}, m_TargetData: {m_TargetData}");
        }

        void GroundCheck()
        {
            m_IsGrounded = false;
            float checkDistance = m_CharacterController.skinWidth + 0.03f;
            float radius = m_CharacterController.radius;
            Vector3 start = transform.position + m_CharacterController.center - m_CharacterController.height / 2 * Vector3.up + radius * Vector3.up;
            Vector3 end = transform.position + m_CharacterController.center + m_CharacterController.height / 2 * Vector3.up - radius * Vector3.up;
            if (Physics.CapsuleCast(start, end, m_CharacterController.radius,
                Vector3.down, out RaycastHit hitInfo, checkDistance, GroundLayer, QueryTriggerInteraction.Ignore)
            ) {
                if (hitInfo.collider.gameObject != gameObject) {
                    m_IsGrounded = true;
                }
            }
        }

        void RotationBody()
        {
            float targetAngle = 0f;
            if (RotationType == CouplingMode.Coupled) {
                targetAngle = m_PlayerRuntimeData.CameraRoot.eulerAngles.y;
            } else if (RotationType == CouplingMode.Decoupled) {
                var velocityX = m_Velocity.x;
                var velocityZ = m_Velocity.z;
                if (velocityX == 0f && velocityZ == 0f) {
                    return;
                }
                targetAngle = Mathf.Atan2(velocityX, velocityZ) * Mathf.Rad2Deg;
            }
            float degree = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref m_RotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, degree, 0f);
        }

        public bool IsMoved()
        {
            return m_InputGlobal.magnitude > 0;
        }

        void OnAim(Event.AimEvent evt)
        {
            UpdateTargetSpeed(m_PlayerRuntimeData.State);
        }

        void OnMove(Vector2 moveInput)
        {
            m_RawInput = moveInput;
            m_Input = new Vector3(m_RawInput.x, 0, m_RawInput.y);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}
