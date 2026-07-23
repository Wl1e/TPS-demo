using System.Collections;
using UnityEngine;

namespace TPSDemo
{
    /// <summary>
    /// 受伤时，将攻击者发送给周围Enemy
    /// </summary>
	public class AlarmController: MonoBehaviour
	{
        private EnemyController m_Owner;
        private SkillController m_Skill = null;

        public event System.Action AlarmFinished;

        private Coroutine m_AlarmCo;

        private void Start()
        {
            if(TryGetComponent(out m_Owner)) {
                m_Owner.Health.OnTakeDamaged += OnTakeDamaged;
                m_Skill = m_Owner.SkillController;
                //m_Skill.CurrentSkill.StateChanged;
            }
        }

        private void OnTakeDamaged(DamageInfo info)
        {
            if (m_Skill) {
                m_Skill.PerformSkill(info.Attacker.transform, 1003);
            }
        }
    }
}
