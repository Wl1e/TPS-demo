using System.Collections;
using UnityEngine;

namespace TPSDemo.FSM
{
	public class LedgeState: StateBase<PlayerStateMachine>
    {
        public float MantleDuration = 0.5f;
        public bool InMantle { get; private set; }
        public bool FinishMantle { get; private set; }
        Coroutine m_MantleCoroutine;

        public LedgeState(PlayerStateMachine stateMachine) : base(stateMachine, "Ledge")
        {
        }

        public override void OnEnter()
        {
            InMantle = false;
            FinishMantle = false;
            m_StateMachine.RuntimeData.AniParameter.IsLedge = true;
        }

        public override void OnExit()
        {
            m_StateMachine.RuntimeData.AniParameter.IsLedge = false;
            m_MantleCoroutine = null;
            InMantle = true;
            FinishMantle = false;
        }

        public override void Update()
        {
            if(InMantle) {
                return;
            }
            var yInput = m_StateMachine.Movement.RawInput.y;
            if (m_MantleCoroutine == null && yInput > 0.5f && m_StateMachine.WantJump) {
                m_StateMachine.WantJump = false;
                m_MantleCoroutine = m_StateMachine.StartCoroutine(Mantle());
            }
        }

        IEnumerator Mantle()
        {
            InMantle = true;
            m_StateMachine.RuntimeData.AniParameter.IsMantle = true;
            m_StateMachine.Movement.MovementLock.Increase();
            yield return new WaitForSeconds(MantleDuration); // Mantle animation duration
            m_StateMachine.Controller.CharacterController.enabled = false;
            m_StateMachine.Movement.SetPosition(m_StateMachine.Controller.ClimbController.TopHitInfo.point + Vector3.up * 0.1f);
            m_StateMachine.Controller.CharacterController.enabled = true;
            m_StateMachine.Movement.MovementLock.Decrease();
            FinishMantle = true;
            InMantle = false;
            m_MantleCoroutine = null;
        }
    }
}
