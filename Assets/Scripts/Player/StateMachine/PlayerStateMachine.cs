using UnityEngine;
using System.Collections.Generic;
using System;

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

        void Awake()
        {
            m_Movement = GetComponent<PlayerMovement>();
            m_PlayerController = GetComponent<PlayerController>();
        }

        void UpdatePlayerState(PlayerMovementState state)
        {
            m_State = state;
            RuntimeData.State = m_State;
            RuntimeData.OnStateChanged?.Invoke(m_State);
        }

        public override void InitializeFSM()
        {
            IdleState idleState = new IdleState(this);
            WalkState walkState = new WalkState(this);
            JumpState jumpState = new JumpState(this);
            CrouchState crouchState = new CrouchState(this);
            SprintState sprintState = new SprintState(this);
            ClimbState climbState = new ClimbState(this);
            LedgeState ledgeState = new LedgeState(this);

            idleState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Idle);
            walkState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Walk);
            jumpState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Jump);
            crouchState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Crouch);
            sprintState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Sprint);
            climbState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Climb);
            ledgeState.OnStateEntered += () => UpdatePlayerState(PlayerMovementState.Ledge);

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

            // crouch
            crouchState.AddTransition(
                idleState,
                new FuncPredicate(() => !WantCrouch || WantJump)
            );
            crouchState.AddTransition(
                sprintState,
                new FuncPredicate(() => WantSprint && m_Movement.IsMoved())
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

            // Ledge
            ledgeState.AddTransition(
                idleState,
                new FuncPredicate(() => m_Movement.IsGrounded || !m_PlayerController.ClimbController.CanLedge || ledgeState.FinishMantle)
            );
            //ledgeState.AddTransition(
            //    idleState,
            //    new FuncPredicate(() => WantJump)
            //);


            AddState(idleState);
            AddState(walkState);
            AddState(jumpState);
            AddState(crouchState);
            AddState(sprintState);
            AddState(climbState);
            AddState(ledgeState);
            ChangeState(idleState);

            OnStateChanged += (string pre, string cur) => print($"{pre} => {cur}");
        }
        public void StartFSM()
        {
            Run();
        }
    }
}
