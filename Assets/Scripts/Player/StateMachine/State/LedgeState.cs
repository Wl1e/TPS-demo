using System.Collections;
using UnityEngine;

namespace TPSDemo.FSM
{
	public class LedgeState: StateBase<PlayerStateMachine>
    {
        public Vector3 StartPos = Vector3.zero;
        public Vector3 MidPos = new(0f, 1.7f, 0f);
        public Vector3 EndPos = new(0f, 1.7f, 0.5f);
        public float NowTime = 0f;
        public float MidTime = 0.5f;
        public float EndTime = 0.9f;

        public float MantleDuration = 0.5f;
        public bool InMantle { get; private set; }
        public bool FinishMantle { get; private set; }
        Coroutine m_MantleCoroutine;

        private Vector3 m_TargetPoint;

        public LedgeState(PlayerStateMachine stateMachine) : base(stateMachine, "Ledge")
        {
        }

        public override void OnEnter()
        {
            InMantle = false;
            FinishMantle = false;
            m_StateMachine.RuntimeData.AniParameter.IsMantle = false;
            m_StateMachine.RuntimeData.AniParameter.IsLedge = true;
            m_StateMachine.RuntimeData.AniParameter.DisableAimLayer = true;
        }

        public override void OnExit()
        {
            m_StateMachine.RuntimeData.AniParameter.IsLedge = false;
            m_StateMachine.RuntimeData.AniParameter.DisableAimLayer = false;
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
                m_MantleCoroutine = m_StateMachine.StartCoroutine(MantleCoroutine());
            }
        }

        IEnumerator MantleCoroutine()
        {
            InMantle = true;
            NowTime = 0f;
            m_StateMachine.RuntimeData.AniParameter.IsMantle = true;
            m_StateMachine.Movement.MovementLock.Increase();
            m_StateMachine.Controller.CharacterController.enabled = false;

            Vector3 origin = m_StateMachine.Movement.transform.position;
            while (NowTime < EndTime) {
                UpdatePosition(origin);
                yield return null;
                NowTime += Time.deltaTime;
            }

            m_TargetPoint = Vector3.zero;

            m_StateMachine.Controller.CharacterController.enabled = true;
            m_StateMachine.Movement.MovementLock.Decrease();
            FinishMantle = true;
            InMantle = false;
            m_MantleCoroutine = null;
        }

        void UpdatePosition(Vector3 origin)
        {
            if (NowTime < MidTime) {
                Vector3 Pos = origin +
                    m_StateMachine.Movement.transform.rotation * Vector3.Lerp(StartPos, MidPos, NowTime / MidTime);
                m_StateMachine.Movement.SetPosition(Pos);
            } else if(NowTime < EndTime) {
                Vector3 Pos = origin +
                    m_StateMachine.Movement.transform.rotation * Vector3.Lerp(MidPos, EndPos, (NowTime - MidTime) / (EndTime - MidTime));
                m_StateMachine.Movement.SetPosition(Pos);
            }
            
        }
    }
}
