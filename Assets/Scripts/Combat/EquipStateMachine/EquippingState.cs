
namespace TPSDemo.Combat.StateMachine
{
    public class EquippingState : FSM.StateBase<TPSDemo.EquipStateMachine>
    {
        public EquippingState(TPSDemo.EquipStateMachine stateMachine) : base(stateMachine, "Equipping")
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
