
namespace Combat.StateMachine
{
    public class NoneState : FSM.StateBase<EquipStateMachine>
    {
        public NoneState(EquipStateMachine stateMachine) : base(stateMachine, "None")
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
