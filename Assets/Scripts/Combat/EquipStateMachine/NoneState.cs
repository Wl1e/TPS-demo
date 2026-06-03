
namespace TPSDemo.Combat.StateMachine
{
    public class NoneState : FSM.StateBase<TPSDemo.EquipStateMachine>
    {
        public NoneState(TPSDemo.EquipStateMachine stateMachine) : base(stateMachine, "None")
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
