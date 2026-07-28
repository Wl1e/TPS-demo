using UnityEngine;

namespace TPSDemo
{
	public class SkyDiveSkill: SkillBase
	{
        public const int SkillId = (int)SkillName.SkyDiveSkill;
        public override int Id => SkillId;

        private float RiseSpeed => (Config as SkyDiveSkillConfig).RiseSpeed;
        private float DiveSpeed => (Config as SkyDiveSkillConfig).DiveSpeed;
        private float AoeRadius => (Config as SkyDiveSkillConfig).AoeRadius;

        private LayerMask TargetLayer => (Config as SkyDiveSkillConfig).TargetLayer;

        private Vector3 m_RiseStartPos;

        private float m_RiseTime = 0f;
        private float m_WaitTime = 0f;

        private Vector3 m_DiveDir = Vector3.negativeInfinity;

        private Vector3 m_EndPosition;

        public override bool ValidPerform(Transform target)
        {
            if (target == null) {
                return false;
            }

            var sqrDist = (target.position - m_EnemyController.transform.position).sqrMagnitude;
            return !InCD() && InRange(sqrDist, Config.AttackRange);
        }

        public override void Prepare(Transform target)
        {
            base.Prepare(target);
            m_RiseStartPos = m_EnemyController.transform.position;

            m_RiseTime = Config.ActiveTime;
            m_WaitTime = Random.Range(1f, 3f);
            m_DiveDir = Vector3.negativeInfinity;

            var agent = m_EnemyController.Agent;
            agent.isStopped = true;        // 停导航
            agent.updatePosition = false;  // ❗ 禁止 Agent 同步位置
            agent.updateRotation = false;  // 可选：也关旋转同步
        }

        protected override void Perform(float deltaTime)
        {
            var dir = m_Target.position - m_EnemyController.transform.position;
            dir.y = 0f;
            var rotation = Quaternion.LookRotation(dir);
            m_EnemyController.transform.rotation = rotation;
            if (m_Duration < m_RiseTime) {
                // 阶段1：垂直升空
                float t = m_Duration / m_RiseTime;
                float height = Mathf.Lerp(0f, m_RiseTime * RiseSpeed, t);
                m_EnemyController.transform.position = new Vector3(
                    m_RiseStartPos.x,
                    m_RiseStartPos.y + height,
                    m_RiseStartPos.z
                );
            } else if (m_Duration <= m_RiseTime + m_WaitTime) {
                // 阶段2：等待
                return;
            } else {
                if(float.IsNegativeInfinity(m_DiveDir.x)) {
                    m_DiveDir = Vector3.Normalize(m_Target.position - m_EnemyController.transform.position);
                    //Debug.Log("m_DiveDir: " + m_DiveDir);
                }

                // 阶段3：俯冲向目标
                m_EnemyController.transform.position += m_DiveDir * DiveSpeed * Time.deltaTime;
                //Debug.Log("position: " + m_EnemyController.transform.position);

                // 落地判定
                if (CheckGround()) {
                    var agent = m_EnemyController.Agent;
                    agent.Warp(m_EndPosition);
                    ApplyAoeDamage();
                    ChangeState(SkillState.Recovery);
                }
            }
        }

        protected override void End()
        {
            base.End();
            var agent = m_EnemyController.Agent;
            agent.isStopped = false;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        //  => m_EnemyController.Agent.isOnNavMesh && !m_EnemyController.Agent.isOnOffMeshLink;
        private bool CheckGround()
        {
            //NavMeshHit hit;
            // 从当前位置往下采样 NavMesh，距离容差内就算"着地"
            var land = UnityEngine.AI.NavMesh.SamplePosition(
                m_EnemyController.transform.position,
                out var hit,
                DiveSpeed * Time.deltaTime, // 从当前位置向下 1 单位内
                m_EnemyController.Agent.areaMask // 用 Agent 自身的可走区域
            );
            m_EndPosition = hit.position;
            return land;
        }

        private void ApplyAoeDamage()
        {
            var hits = Physics.OverlapCapsule(
                m_EnemyController.transform.position,
                m_EnemyController.transform.position + Vector3.up * 0.3f,
                AoeRadius,
                TargetLayer,
                QueryTriggerInteraction.Ignore
            );

            Debug.Log($"Layer: {TargetLayer}");

            foreach (var hit in hits) {
                Debug.Log($"Hit: {hit}");
                if (hit.TryGetComponent<Damageable>(out var damageable)) {
                    damageable.InflictDamage(
                        new DamageInfo {
                            Attacker = m_EnemyController.gameObject,
                            Damage = Config.Damage,
                            Point = damageable.transform.position
                        }
                    );
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterSelf() => SkillFactory.Register<SkyDiveSkill>(SkillId);
    }
}
