
namespace Combat.StateMachine
{
    public class UnequippingState : FSM.StateBase<EquipStateMachine>
    {
        public UnequippingState(EquipStateMachine stateMachine) : base(stateMachine, "Unequipping")
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
