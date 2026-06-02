
namespace Combat.StateMachine
{
    public class EquippingState : FSM.StateBase<EquipStateMachine>
    {
        public EquippingState(EquipStateMachine stateMachine) : base(stateMachine, "Equipping")
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
