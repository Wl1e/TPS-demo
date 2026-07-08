using UnityEngine;

namespace TPSDemo
{
    public class SkillAttacker: AttackerBase
    {
        [Tooltip("技能列表")]
        [SerializeField] System.Collections.Generic.List<SkillBase> m_Skills;

        public override void Attack(Transform target)
        {
        }

        public void Attack(Transform target, int skillIdx)
        {
            if (skillIdx < 0 || skillIdx >= m_Skills.Count) {
                return;
            }
            var skill = m_Skills[skillIdx];

            WhenAttack();
        }

    }
}
