using UnityEngine;

namespace TPSDemo
{
    // 好像没办法主动注册Skill...
	public class ChargeSkill: SkillBase
	{
        private const int m_Id = 1001;
        public static int SkillId => m_Id;

        public float ChargeSpeed = 1f;
        private Vector3 m_TargetPos = Vector3.zero;
        private Vector3 m_LastPos = Vector3.negativeInfinity;
        private float m_Threshold = 0.1f;

        [RuntimeInitializeOnLoadMethod]
        private static void RegisterSelf() => SkillFactory.Register<ChargeSkill>(SkillId);

        //public override void Initialize(EnemyController enemy, SkillConfig config)
        //{
        //    base.Initialize(enemy, config);
        //}

        public override bool ValidPerform(Transform target)
        {
            var sqrDistance = Vector3.SqrMagnitude(target.position - m_EnemyController.transform.position);
            var sqrAttackRange = Config.AttackRange * Config.AttackRange;
            //Debug.Log($"sqrDistance: {sqrDistance}, " +
            //    $"m_Threshold: {m_Threshold * m_Threshold}, " +
            //    $"sqrAttackRange: {sqrAttackRange}");
            return sqrDistance > m_Threshold * m_Threshold &&
                sqrDistance >= sqrAttackRange.x &&
                sqrDistance <= sqrAttackRange.y;
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
            Debug.Log("charge skill perform");
            if((m_LastPos - m_EnemyController.transform.position).sqrMagnitude < m_Threshold * m_Threshold) {
                Debug.Log("charge skill finished");
                m_State = SkillState.Recovery;
                return;
            }
            Vector3 dir = (m_TargetPos - m_EnemyController.transform.position).normalized;
            m_EnemyController.Agent.isStopped = true;
            m_EnemyController.Agent.Move(ChargeSpeed * Time.deltaTime * dir);
        }

        public override void End()
        {
            m_EnemyController.EnemyHitbox.SetEnable(false);
        }
	}
}
