using UnityEngine;
using System.Collections.Generic;

namespace TPSDemo
{
    
	public class SkillController: MonoBehaviour
	{
        [System.Serializable]
        public struct SkillEntry
        {
            public int Id;
            public SkillConfig Config;
        }

        // 用SerializeReference更简单，但是需要写一个Editor类支持
        [Tooltip("技能列表（放置顺序代表优先级）")]
        public List<SkillEntry> Configs = new();
        private readonly List<ISkill> m_Skills = new();

        [Tooltip("随机失败概率（可以执行Skill但是不执行，增加随机性，但是不会全失败）")]
        [SerializeField] private float m_FailRatio;

        private ISkill m_CurrentSkill;

        public bool IsFinished { get; private set; } = true;

        private void Start()
        {
            var enemy = GetComponent<EnemyController>();
            foreach (var entry in Configs) {
                ISkill skill = SkillFactory.Create(entry.Id);
                skill.Initialize(enemy, entry.Config);
                m_Skills.Add(skill);
            }
        }
        private void Update()
        {
            if (IsFinished || m_CurrentSkill == null) {
                return;
            }
            m_CurrentSkill.Update(Time.deltaTime);
            if(!m_CurrentSkill.IsRunning) {
                IsFinished = true;
            }
        }

        private ISkill GetSkill(int skillIdx)
        {
            if(skillIdx < 0 || skillIdx >= m_Skills.Count) {
                return null;
            }
            return m_Skills[skillIdx];
        }

        public int GetBestSkill(Transform target)
        {
            int skillIdx = -1;
            for (int idx = 0; idx < m_Skills.Count; idx++) {
                var skill = m_Skills[idx];
                if (skill.ValidPerform(target)) {
                    if (skillIdx == -1) {
                        skillIdx = idx;
                    }
                    if (Random.Range(0, 100) < m_FailRatio) {
                        continue;
                    }
                    return idx;
                }
            }
            return skillIdx;
        }

        public bool ValidPerformSkill(Transform target, int skillIdx)
        {
            var skill = GetSkill(skillIdx);
            if (skill == null) {
                Debug.Log($"不存在 {skillIdx}号 Skill");
                return false;
            }

            return skill.ValidPerform(target);
        }

        public void PerformSkill(Transform target, int skillIdx)
        {
            var skill = GetSkill(skillIdx);
            if (skill == null) {
                Debug.Log("不存在Skill " + skillIdx);
                return;
            }

            m_CurrentSkill = skill;
            m_CurrentSkill.Prepare(target);
            IsFinished = false;
        }

    }
}
