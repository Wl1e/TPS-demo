
using UnityEngine;

namespace Combat.StateMachine
{
    public class SwitchingState : FSM.StateBase<EquipStateMachine>
    {
        float m_Duration = 0.15f;
        float m_Time = 0f;
        public SwitchingState(EquipStateMachine stateMachine) : base(stateMachine, "Switching")
        {
        }

        public override void OnEnter() { }
        public override void OnExit()
        {
        }
        public override void Update()
        {
            m_Time += Time.time;
            if(m_Time >= m_Duration) {
                Exit();
            }
        }
    }
} // namespace Combat.EquipStateMachine
