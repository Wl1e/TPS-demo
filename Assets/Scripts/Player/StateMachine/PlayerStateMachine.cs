using UnityEngine;

namespace TPSDemo
{
using FSM;
    public class PlayerStateMachine : StateMachine
    {
        PlayerController m_PlayerController;
        public PlayerController Controller => m_PlayerController;
        public PlayerRuntimeData RuntimeData => m_PlayerController.RuntimeData;
        private PlayerMovement m_Movement;
        public PlayerMovement Movement => m_Movement;

        PlayerMovementState m_State = PlayerMovementState.Idle;
        public PlayerMovementState State => m_State;

        [HideInInspector] public bool WantJump = false;
        [HideInInspector] public bool WantSprint = false;
        [HideInInspector] public bool WantCrouch = false;

        public event System.Action<PlayerMovementState> OnStateChanged;

        // 暂时给Mantle调试使用
        public Vector3 StartPos = Vector3.zero;
        public Vector3 MidPos = new Vector3(0f, 1.7f, 0f);
        public Vector3 EndPos = new Vector3(0f, 1.7f, 0.5f);
        public float NowTime = 0f;
        public float MidTime = 0.5f;
        public float EndTime = 0.9f;
        void Awake()
        {
            m_Movement = GetComponent<PlayerMovement>();
            m_PlayerController = GetComponent<PlayerController>();
        }

        void UpdatePlayerState(PlayerMovementState state)
        {
            m_State = state;
            RuntimeData.State = m_State;
            OnStateChanged?.Invoke(m_State);
        }

        public override void InitializeFSM()
        {
            IdleState idleState = new(this);
            WalkState walkState = new(this);
            JumpState jumpState = new(this);
            CrouchState crouchState = new(this);
            SprintState sprintState = new(this);
            ClimbState climbState = new(this);
            LedgeState ledgeState = new(this);
            DiedState diedState = new(this);

            idleState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Idle);
            walkState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Walk);
            jumpState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Jump);
            crouchState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Crouch);
            sprintState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Sprint);
            climbState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Climb);
            ledgeState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Ledge);
            diedState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Died);

            // idle
            idleState.AddTransition(
                walkState,
                new FuncPredicate(() => m_Movement.IsMoved())
            );
            idleState.AddTransition(
                jumpState,
                new FuncPredicate(() => WantJump && m_Movement.IsGrounded)
            );
            idleState.AddTransition(
                crouchState,
                new FuncPredicate(() => WantCrouch)
            );
            idleState.AddTransition(
                sprintState,
                new FuncPredicate(() => WantSprint && m_Movement.IsMoved())
            );
            idleState.AddTransition(
                diedState,
                new FuncPredicate(() => RuntimeData.IsDied)
            );

            // walk
            walkState.AddTransition(
                idleState,
                new FuncPredicate(() => !m_Movement.IsMoved())
            );
            walkState.AddTransition(
                jumpState,
                new FuncPredicate(() => WantJump && m_Movement.IsGrounded)
            );
            walkState.AddTransition(
                crouchState,
                new FuncPredicate(() => WantCrouch)
            );
            walkState.AddTransition(
                sprintState,
                new FuncPredicate(() => WantSprint && m_Movement.IsMoved() && !RuntimeData.IsAiming)
            );
            walkState.AddTransition(
                climbState,
                new FuncPredicate(() => m_PlayerController.ClimbController.CanClimb)
            );
            walkState.AddTransition(
                diedState,
                new FuncPredicate(() => RuntimeData.IsDied)
            );

            // jump
            jumpState.AddTransition(
                idleState,
                new FuncPredicate(() => m_Movement.IsGrounded && !m_Movement.IsMoved())
            );
            jumpState.AddTransition(
                walkState,
                new FuncPredicate(() => m_Movement.IsGrounded && !WantSprint && m_Movement.IsMoved())
            );
            jumpState.AddTransition(
                crouchState,
                new FuncPredicate(() => m_Movement.IsGrounded && WantCrouch)
            );
            jumpState.AddTransition(
                sprintState,
                new FuncPredicate(() => m_Movement.IsGrounded && WantSprint && m_Movement.IsMoved())
            );
            jumpState.AddTransition(
                climbState,
                new FuncPredicate(() => m_PlayerController.ClimbController.CanClimb)
            );
            jumpState.AddTransition(
                diedState,
                new FuncPredicate(() => RuntimeData.IsDied)
            );

            // crouch
            crouchState.AddTransition(
                idleState,
                new FuncPredicate(() => !WantCrouch || WantJump)
            );
            crouchState.AddTransition(
                sprintState,
                new FuncPredicate(() => WantSprint && m_Movement.IsMoved())
            );
            crouchState.AddTransition(
                diedState,
                new FuncPredicate(() => RuntimeData.IsDied)
            );

            // sprint
            sprintState.AddTransition(
                idleState,
                new FuncPredicate(() => !WantSprint || !m_Movement.IsMoved())
            );
            sprintState.AddTransition(
                walkState,
                new FuncPredicate(() => (!WantSprint && m_Movement.IsMoved()) || (RuntimeData.IsAiming && m_Movement.IsMoved()))
            );
            sprintState.AddTransition(
                jumpState,
                new FuncPredicate(() => WantJump && m_Movement.IsGrounded)
            );
            sprintState.AddTransition(
                diedState,
                new FuncPredicate(() => RuntimeData.IsDied)
            );

            // climb
            climbState.AddTransition(
                idleState,
                new FuncPredicate(() => m_Movement.IsGrounded || (!m_PlayerController.ClimbController.CanClimb && !m_PlayerController.ClimbController.CanLedge))
            );
            climbState.AddTransition(
                jumpState,
                new FuncPredicate(() => WantJump)
            );
            climbState.AddTransition(
                ledgeState,
                new FuncPredicate(() => m_PlayerController.ClimbController.CanLedge)
            );
            climbState.AddTransition(
                diedState,
                new FuncPredicate(() => RuntimeData.IsDied)
            );

            // Ledge
            ledgeState.AddTransition(
                idleState,
                new FuncPredicate(
                    () => m_Movement.IsGrounded ||
                    (!m_PlayerController.ClimbController.CanLedge && !ledgeState.InMantle) ||
                    ledgeState.FinishMantle
                )
            );
            //ledgeState.AddTransition(
            //    idleState,
            //    new FuncPredicate(() => WantJump)
            //);

            // died
            diedState.AddTransition(
                idleState,
                new FuncPredicate(() => !RuntimeData.IsDied)
            );

            AddState(idleState);
            AddState(walkState);
            AddState(jumpState);
            AddState(crouchState);
            AddState(sprintState);
            AddState(climbState);
            AddState(ledgeState);
            AddState(diedState);
            ChangeState(idleState);

            //OnStateChanged += (string pre, string cur) => Debug.Log($"{pre} => {cur}");
        }
        public void StartFSM()
        {
            Run();
        }
    }
}
