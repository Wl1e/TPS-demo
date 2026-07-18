using UnityEngine;

namespace TPSDemo.FSM
{

    public class WalkState : StateBase<PlayerStateMachine>
    {
        public WalkState(PlayerStateMachine stateMachine) : base(stateMachine, "Walk")
        {
            m_StateMachine.Controller.AudioEffectPlayer.LoopAudio("Movement", m_StateMachine.Controller.m_MovementAudio);
        }

        public override void OnEnter()
        {
            m_StateMachine.WantSprint = false;
            m_StateMachine.Controller.AudioEffectPlayer.Play("Movement", float.PositiveInfinity, Vector3.zero, Quaternion.identity, true);
        }

        public override void OnExit()
        {
            m_StateMachine.Controller.AudioEffectPlayer.StopLoopAudio("Movement");
        }

        public override void Update()
        {
            // Handle walk logic here
        }
    }

}
