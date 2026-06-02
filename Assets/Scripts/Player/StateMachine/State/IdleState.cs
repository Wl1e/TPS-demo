
using UnityEngine;

namespace FSM
{
    public class IdleState : StateBase<PlayerStateMachine>
    {
        public IdleState(PlayerStateMachine stateMachine) : base(stateMachine, "Idle")
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
            // Handle idle logic here
        }
    }
}
