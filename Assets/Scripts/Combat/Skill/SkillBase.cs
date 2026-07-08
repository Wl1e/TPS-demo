using System;
using System.Collections;
using UnityEngine;

namespace TPSDemo
{
    public interface ISkill
    {
        public string Name { get; }
        public static int SkillId { get; }
        public bool IsRunning { get; }
        public void Initialize(EnemyController enemy, SkillConfig config);
        /// <summary>
        /// Update前执行，重置状态，设置释放对象
        /// </summary>
        /// <param name="target"></param>
        public void Prepare(Transform target);
        public void Update(float deltaTime);
        public bool ValidPerform(Transform target);
    }

    public abstract class SkillBase: ISkill
    {
        public enum SkillState
        {
            None,
            Windup,
            Running,
            Recovery,
            Finished,
        }

        public string Name => m_Config.Name;
        public bool IsRunning => m_State != SkillState.None && m_State != SkillState.Finished;

        /// <summary>
        /// 配置文件
        /// </summary>
        private SkillConfig m_Config;

        // 目前由Enemy持有，后续如果要扩展到Player，可以改为GO或Actor
        protected EnemyController m_EnemyController;

        protected SkillState m_State = SkillState.None;
        protected SkillConfig Config => m_Config;

        protected float m_Duration = 0f;
        private float m_WindupDuration = 0f;
        private float m_RecoveryDuration = 0f;

        protected Transform m_Target;

        public virtual void Initialize(EnemyController enemy, SkillConfig config)
        {
            m_EnemyController = enemy;
            m_Config = config;
        }

        public virtual void Prepare(Transform target)
        {
            if(IsRunning) {
                return;
            }
            m_Target = target;
            m_State = SkillState.Windup;
            m_Duration = 0f;
            m_WindupDuration = m_Config.WindupTime;
            m_RecoveryDuration = m_Config.RecoveryTime;
        }

        public abstract void End();

        public void Update(float deltaTime)
        {
            if(m_State == SkillState.Windup) {
                Debug.Log("进入前摇");
                if (Windup(deltaTime)) {
                    return;
                } else {
                    m_State = SkillState.Running;
                }
            }

            if(m_State == SkillState.Running) {
                Debug.Log("释放");
                m_Duration += deltaTime;
                Perform(deltaTime);
                if(m_Duration >= m_Config.ActiveTime) {
                    m_State = SkillState.Recovery;
                }
            }

            if(m_State == SkillState.Recovery) {
                Debug.Log("进入后摇");
                if (Recovery(deltaTime)) {
                    return;
                } else {
                    m_State = SkillState.Finished;
                }
            }
        }

        /// <summary>
        /// Skill实际执行（子类重写）
        /// </summary>
        /// <param name="target"> 施放目标 </param>
        /// <returns></returns>
        protected abstract void Perform(float deltaTime);

        /// <summary>
        /// 前摇
        /// </summary>
        protected bool Windup(float deltaTime)
        {
            m_WindupDuration -= deltaTime;
            return m_WindupDuration > 0f;
        }
        /// <summary>
        /// 后摇
        /// </summary>
        protected bool Recovery(float deltaTime)
        {
            m_RecoveryDuration -= deltaTime;
            return m_RecoveryDuration > 0f;
        }
        public abstract bool ValidPerform(Transform target);
    };

    //static public class SkillGenerator
    //{
    //    static private SkillCreateDelegate
    //    static private System.Collections.Generic.Dictionary<int, Delegate>
    //    static public 
    //}
}
