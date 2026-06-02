using UnityEngine;

namespace FSM
{

    public class CrouchState : StateBase<PlayerStateMachine>
    {

        public CrouchState(PlayerStateMachine stateMachine) : base(stateMachine, "Crouch")
        {
        }

        public override void OnEnter()
        {
            m_StateMachine.RuntimeData.AniParameter.IsCrouch = true;
        }

        public override void OnExit()
        {
            m_StateMachine.RuntimeData.AniParameter.IsCrouch = false;
            m_StateMachine.WantCrouch = false;
        }

        public override void Update()
        {
            // Handle crouch logic here
        }
    }

}
