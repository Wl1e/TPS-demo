
namespace TPSDemo.FSM
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
            m_StateMachine.RuntimeData.AniParameter.DisableAimLayer = true;

            m_StateMachine.RuntimeData.DisableCombat = true;
            m_StateMachine.RuntimeData.CanUseActiveItem = false;
        }

        public override void OnExit()
        {
            m_StateMachine.RuntimeData.AniParameter.IsClimb = false;
            m_StateMachine.RuntimeData.AniParameter.DisableAimLayer = false;

            m_StateMachine.RuntimeData.DisableCombat = false;
            m_StateMachine.RuntimeData.CanUseActiveItem = true;
        }

        public override void Update()
        {
            // Handle jump logic here
            // maybe?
        }
    }
}
