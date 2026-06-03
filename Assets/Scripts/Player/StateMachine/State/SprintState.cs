
using UnityEngine;

namespace TPSDemo.FSM
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
            // 清空导致Jump回到Sprint时动画从头播放了
            m_StateMachine.RuntimeData.AniParameter.IsSprint = false;
        }

        public override void Update()
        {
            // Handle sprint logic here
        }
    }
}
