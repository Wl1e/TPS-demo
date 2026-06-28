
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
            m_StateMachine.RuntimeData.CanUseActiveItem = false;
        }

        public override void OnExit()
        {
            // 清空导致Jump回到Sprint时动画从头播放了
            m_StateMachine.RuntimeData.AniParameter.IsSprint = false;
            m_StateMachine.RuntimeData.CanUseActiveItem = true;
        }

        public override void Update()
        {
            // Handle sprint logic here
        }
    }
}
