using System.Collections;
using UnityEngine;

namespace TPSDemo
{
    public abstract class SkillBase
    {
        public enum SkillState
        {
            None,
            Windup,
            Running,
            Recovery,
            Finished,
        }

        /// <summary>
        /// 配置文件
        /// </summary>
        private SkillConfig m_Config;

        // 目前由Enemy持有，后续如果要扩展到Player，可以改为GO或Actor
        protected EnemyController m_EnemyController;

        protected SkillState m_State = SkillState.None;
        protected SkillConfig Config => m_Config;

        public bool IsRunning => m_State != SkillState.None && m_State != SkillState.Finished;

        public virtual void Initialize(SkillConfig config) => m_Config = config;

        public IEnumerator ExecuteSkill(Transform target)
        {
            yield return Windup();

            m_State = SkillState.Running;
            yield return Perform(target);

            yield return Recovery();

            m_State = SkillState.Finished;
        }

        /// <summary>
        /// Skill实际执行（子类重写）
        /// </summary>
        /// <param name="target"> 施放目标 </param>
        /// <returns></returns>
        protected abstract IEnumerator Perform(Transform target);

        /// <summary>
        /// 前摇
        /// </summary>
        protected IEnumerator Windup()
        {
            m_State = SkillState.Windup;
            yield return new WaitForSeconds(m_Config.WindupTime);
        }
        /// <summary>
        /// 后摇
        /// </summary>
        protected IEnumerator Recovery()
        {
            m_State = SkillState.Recovery;
            yield return new WaitForSeconds(m_Config.RecoveryTime);
        }
    };
}
