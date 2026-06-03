

namespace TPSDemo
{
using Combat.StateMachine;
    public class EquipStateMachine : FSM.StateMachine
    {
        CombatController m_CombatController;

        private void Start()
        {
            m_CombatController = GetComponent<CombatController>();
        }

        public override void InitializeFSM()
        {
            var noneState = new NoneState(this);
            var equippingState = new EquippingState(this);
            var holdingState = new HoldingState(this);
            var switchingState = new SwitchingState(this);
            var unequippingState = new UnequippingState(this);
            var reloadingState = new ReloadingState(this);

            // None m_State
            //noneState.AddTransition(equippingState, )
        }
    }
}
