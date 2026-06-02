
namespace Combat.StateMachine
{
    public class HoldingState : FSM.StateBase<EquipStateMachine>
    {
        readonly float m_Duration;

        public HoldingState(EquipStateMachine machine)
            : base(machine, "Holding") { }

        public override void OnEnter()
        {
        }

        public override void Update()
        {
        }

        public override void OnExit()
        {
        }
    }
} // namespace Combat.EquipStateMachine
