using UnityEngine;
using static UnityEngine.Rendering.STP;

namespace TPSDemo
{
    public class ChargeSkill: SkillBase
	{
        private const int SkillId = (int)SkillName.ChargeSkill;
        public override int Id => SkillId;

        public float ChargeSpeed => (Config as ChargeSkillConfig).ChargeSpeed;
        private Vector3 m_TargetPos = Vector3.zero;
        private Vector3 m_LastPos = Vector3.negativeInfinity;
        private float Threshold => (Config as ChargeSkillConfig).Threshold;

        private Hitbox m_Hitbox;
        private bool m_Collision = false;

        [RuntimeInitializeOnLoadMethod]
        private static void RegisterSelf() => SkillFactory.Register<ChargeSkill>(SkillId);

        public override void Initialize(EnemyController enemy, SkillConfig config)
        {
            base.Initialize(enemy, config);
            m_Hitbox = m_EnemyController.EnemyHitbox;
            m_Hitbox.OnCollision += OnHitboxCollision;
        }

        private void OnHitboxCollision(Damageable obj) => m_Collision = true;

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
            // 撞到Player
            if(m_Collision) {
                ChangeState(SkillState.Recovery);
                return;
            }

            if((m_LastPos - m_EnemyController.transform.position).sqrMagnitude <= Threshold * Threshold ||
                m_Duration >= Config.ActiveTime) {
                ChangeState(SkillState.Recovery);
                return;
            }
            m_LastPos = m_EnemyController.transform.position;

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
