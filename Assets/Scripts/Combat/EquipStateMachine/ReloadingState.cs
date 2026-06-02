
namespace Combat.StateMachine
{
    public class ReloadingState : FSM.StateBase<EquipStateMachine>
    {
        public ReloadingState(EquipStateMachine stateMachine) : base(stateMachine, "Reloading")
        {
        }

        public override void OnEnter() { }
        public override void OnExit()
        {
        }
        public override void Update()
        {
        }
    }
} // namespace Combat.EquipStateMachine
