
namespace TPSDemo.Combat.StateMachine
{
    public class UnequippingState : FSM.StateBase<TPSDemo.EquipStateMachine>
    {
        public UnequippingState(TPSDemo.EquipStateMachine stateMachine) : base(stateMachine, "Unequipping")
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
