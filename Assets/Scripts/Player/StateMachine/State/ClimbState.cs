using UnityEngine;

namespace FSM
{
	public class ClimbState: StateBase<PlayerStateMachine>
    {

        public ClimbState(PlayerStateMachine stateMachine) : base(stateMachine, "Climb")
        {
        }

        public override void OnEnter()
        {
            m_StateMachine.WantJump = false;
            m_StateMachine.RuntimeData.AniParameter.IsClimb = true;
        }

        public override void OnExit()
        {
            m_StateMachine.RuntimeData.AniParameter.IsClimb = false;
        }

        public override void Update()
        {
            // Handle jump logic here
        }
    }
}
