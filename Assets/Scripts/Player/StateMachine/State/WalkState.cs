using UnityEngine;

namespace TPSDemo.FSM
{

    public class WalkState : StateBase<PlayerStateMachine>
    {
        public WalkState(PlayerStateMachine stateMachine) : base(stateMachine, "Walk")
        {
        }

        public override void OnEnter()
        {
            m_StateMachine.WantSprint = false;
        }

        public override void OnExit()
        {
        }

        public override void Update()
        {
            // Handle walk logic here
        }
    }

}
