using UnityEngine;
using System.Collections;

namespace TPSDemo
{
	public class ChargeSkill: SkillBase
	{
        public float ChargeSpeed = 1f;

        public override void Initialize(SkillConfig config)
        {
            base.Initialize(config);
        }

        protected override IEnumerator Perform(Transform target)
        {
            Vector3 dir = (target.position - m_EnemyController.transform.position).normalized;
            float duration = 0f;
            m_EnemyController.Agent.isStopped = true;
            while(duration <= Config.ActiveTime) {
                m_EnemyController.Agent.Move(ChargeSpeed * Time.deltaTime * dir);
                duration += Time.deltaTime;
                yield return null;
            }
        }
	}
}
