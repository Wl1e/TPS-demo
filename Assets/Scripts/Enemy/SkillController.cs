using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    
    public enum SkillName: int
    {
        ChargeSkill = 1001,
        SkyDiveSkill = 1002,
        AlarmSkill = 1003,
    }

	public class SkillController: MonoBehaviour
	{
        [System.Serializable]
        public struct SkillEntry
        {
            public SkillName Id;
            public SkillConfig Config;
        }

        // 用SerializeReference更简单，但是需要写一个Editor类支持
        [Tooltip("技能列表（放置顺序代表优先级）")]
        public List<SkillEntry> Configs = new();
        private readonly List<ISkill> m_Skills = new();

        [Tooltip("随机失败概率（可以执行Skill但是不执行，增加随机性，但是不会全失败）")]
        [SerializeField] private float m_FailRatio;

        /// <summary>
        /// 当前执行技能
        /// </summary>
        private ISkill m_CurrentSkill;
        public ISkill CurrentSkill => m_CurrentSkill;

        public bool IsFinished => (m_CurrentSkill == null);

        private PlayableController m_AnimatorController;

        private void Start()
        {
            var enemy = GetComponent<EnemyController>();
            m_AnimatorController = enemy.GetComponentInChildren<PlayableController>();
            foreach (var entry in Configs) {
                ISkill skill = SkillFactory.Create((int)entry.Id);
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
        }

        private ISkill GetSkill(int skillIdx)
        {
            if(skillIdx < 0 || skillIdx >= m_Skills.Count) {
                return null;
            }
            return m_Skills[skillIdx];
        }
        public ISkill GetSkill(SkillName name)
        {
            foreach (var skill in m_Skills) {
                if (skill.Id == (int)name) {
                    return skill;
                }
            }
            return null;
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
            // 当前正在执行
            if (!IsFinished) {
                Debug.Log($"Skill {m_CurrentSkill.Id} 正在执行");
                return false;
            }

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
            m_CurrentSkill.StateChanged += OnSkillStateChanged;
        }

        private void OnSkillStateChanged(SkillBase.SkillState state)
        {
            switch (state) {
                case SkillBase.SkillState.Windup:
                    m_AnimatorController.Play(m_CurrentSkill.Config.WindupAnimation);
                    break;
                case SkillBase.SkillState.Running:
                    m_AnimatorController.Play(m_CurrentSkill.Config.SkillAnimation);
                    break;
                case SkillBase.SkillState.Recovery:
                    m_AnimatorController.Play(m_CurrentSkill.Config.RecoveryAnimation);
                    break;
                case SkillBase.SkillState.Finished:
                    // 很不可靠，要不然就所有skill一直监听
                    m_CurrentSkill.StateChanged -= OnSkillStateChanged;
                    m_AnimatorController.Stop();
                    m_CurrentSkill = null;
                    break;
            }
        }

    }
}
