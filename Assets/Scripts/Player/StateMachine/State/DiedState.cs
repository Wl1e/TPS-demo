using UnityEngine;

namespace TPSDemo.FSM
{
    public class DiedState : StateBase<PlayerStateMachine>
    {
        public DiedState(PlayerStateMachine stateMachine) : base(stateMachine, "Died")
        {
        }

        public override void OnEnter()
        {
            Debug.Log("Enter Died");
            UpdateComponentState(false);
            m_StateMachine.RuntimeData.AniParameter.IsDied = true;
            m_StateMachine.RuntimeData.AniParameter.Death = true;
            m_StateMachine.RuntimeData.DisableCombat = true;
            m_StateMachine.RuntimeData.CanUseActiveItem = false;
        }

        public override void OnExit()
        {
            Debug.Log("Exit Died");
            UpdateComponentState(true);
            m_StateMachine.RuntimeData.AniParameter.IsDied = false;
            m_StateMachine.RuntimeData.DisableCombat = false;
            m_StateMachine.RuntimeData.CanUseActiveItem = true;
        }

        public override void Update()
        {
            // Handle idle logic here
        }

        private void SetComponentEnabled(Behaviour component, bool enabled)
        {
            if (component != null) {
                component.enabled = enabled;
            }
        }

        private void UpdateComponentState(bool enable)
        {
            SetComponentEnabled(m_StateMachine.Controller.Movement, enable);
            //SetComponentEnabled(m_StateMachine.Controller.StateMachine, enable);
            SetComponentEnabled(m_StateMachine.Controller.CameraController, enable);
            if (m_StateMachine.Controller.CharacterController != null) {
                m_StateMachine.Controller.CharacterController.enabled = enable;
            }

            SetComponentEnabled(m_StateMachine.Controller.InteractionController, enable);

            var aim = m_StateMachine.Controller.GetComponentInChildren<AimController>();
            SetComponentEnabled(aim, enable);
            //var weapon = m_StateMachine.Controller.GetComponentInChildren<WeaponManager>();
            //SetComponentEnabled(weapon, enable);
            var combat = m_StateMachine.Controller.GetComponentInChildren<CombatController>();
            SetComponentEnabled(combat, enable);
            SetComponentEnabled(m_StateMachine.Controller.PlayerInputHandler, enable);
        }
    }
}
