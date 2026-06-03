
namespace TPSDemo.Combat.StateMachine
{
    public class ReloadingState : FSM.StateBase<TPSDemo.EquipStateMachine>
    {
        public ReloadingState(TPSDemo.EquipStateMachine stateMachine) : base(stateMachine, "Reloading")
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
