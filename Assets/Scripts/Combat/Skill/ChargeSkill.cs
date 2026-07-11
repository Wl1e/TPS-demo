using UnityEngine;

namespace TPSDemo
{
    public class ChargeSkill: SkillBase
	{
        private const int SkillId = 1001;

        public float ChargeSpeed = 1f;
        private Vector3 m_TargetPos = Vector3.zero;
        private Vector3 m_LastPos = Vector3.negativeInfinity;
        private float m_Threshold = 0.1f;

        [RuntimeInitializeOnLoadMethod]
        private static void RegisterSelf() => SkillFactory.Register<ChargeSkill>(SkillId);

        public override void Initialize(EnemyController enemy, SkillConfig config)
        {
            base.Initialize(enemy, config);
            float.TryParse(Config.Args[0].Value, out ChargeSpeed);
        }

        public override bool ValidPerform(Transform target)
        {
            if (target == null) {
                return false;
            }
            var sqrDistance = Vector3.SqrMagnitude(target.position - m_EnemyController.transform.position);
            return !InCD() && InRange(sqrDistance, Config.AttackRange);
        }

        public override void Prepare(Transform target)
        {
            base.Prepare(target);
            m_TargetPos = target.position;
            var rotation = Quaternion.LookRotation(m_TargetPos - m_EnemyController.transform.position);
            m_EnemyController.transform.rotation = rotation;
            m_EnemyController.EnemyHitbox.SetEnable(true);
        }

        protected override void Perform(float deltaTime)
        {
            if((m_LastPos - m_EnemyController.transform.position).sqrMagnitude < m_Threshold * m_Threshold ||
                m_Duration >= Config.ActiveTime) {
                ChangeState(SkillState.Recovery);
                return;
            }
            Vector3 dir = (m_TargetPos - m_EnemyController.transform.position).normalized;
            m_EnemyController.Agent.isStopped = true;
            m_EnemyController.Agent.Move(ChargeSpeed * Time.deltaTime * dir);
        }

        protected override void End()
        {
            base.End();
            m_EnemyController.EnemyHitbox.SetEnable(false);
        }
	}
}
