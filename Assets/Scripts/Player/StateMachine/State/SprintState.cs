
using UnityEngine;

namespace FSM
{
    public class SprintState : StateBase<PlayerStateMachine>
    {
        public SprintState(PlayerStateMachine stateMachine) : base(stateMachine, "Sprint")
        {
        }

        public override void OnEnter()
        {
            m_StateMachine.RuntimeData.AniParameter.IsSprint = true;
        }

        public override void OnExit()
        {
            m_StateMachine.RuntimeData.AniParameter.IsSprint = false;
        }

        public override void Update()
        {
            // Handle sprint logic here
        }
    }
}
