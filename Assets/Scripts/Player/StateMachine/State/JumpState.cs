
using UnityEngine;

namespace TPSDemo.FSM
{
    public class JumpState : StateBase<PlayerStateMachine>
    {
        public JumpState(PlayerStateMachine stateMachine) : base(stateMachine, "Jump")
        {
        }

        public override void OnEnter()
        {
            m_StateMachine.RuntimeData.AniParameter.Jump = true;
            m_StateMachine.WantJump = false;
        }

        public override void OnExit()
        {
            m_StateMachine.RuntimeData.AniParameter.Jump = false;
            m_StateMachine.WantJump = false;
        }

        public override void Update()
        {
            // Handle jump logic here
        }
    }
}
