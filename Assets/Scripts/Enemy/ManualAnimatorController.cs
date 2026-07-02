using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    /// <summary>
    /// 手动控制 Animator 播放动画，支持 CrossFade 过渡。
    /// 挂在带 Animator 的 GameObject 上即可。
    /// 需要在Animator中加上所有你使用的Type类型的空动画片段，方便替换。
    /// </summary>
    public class ManualAnimatorController : MonoBehaviour
    {
        #region
        public enum AnimationType
        {
            Idle,
            Walk,
            Run,
            Attack,
            Hit
        }

        [System.Serializable]
        public struct AnimatorEntry
        {
            public AnimationType Type;
            public AnimationClip Clip;
        }
        #endregion

        [Header("引用")]
        [SerializeField] private Animator m_Animator = null;

        [SerializeField] private List<AnimatorEntry> m_Clips;

        private AnimatorOverrideController m_Override;

        [Header("设置")]
        [SerializeField] private float m_DefaultBlendDuration = 0.2f;
        [SerializeField] private AnimationType m_DefaultAnimationType = AnimationType.Idle;

        private AnimationType m_CurrentType = AnimationType.Idle;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_Override = new AnimatorOverrideController(m_Animator.runtimeAnimatorController);
            m_Animator.runtimeAnimatorController = m_Override;
            InitializeOverride();
        }

        private bool Valid() => m_Animator != null;

        /// <summary>
        /// 播放指定名称的动画状态，自动 CrossFade 过渡
        /// </summary>
        /// <param name="type">Animator状态</param>
        /// <param name="blendDuration">过渡时间。-1 则使用默认值</param>
        public void Play(AnimationType type, float blendDuration = -1f)
        {
            if(!Valid()) {
                return;
            }
            if(type == m_CurrentType) {
                return;
            }

            m_CurrentType = type;

            m_Animator.speed = 1f;
            float duration = blendDuration >= 0 ? blendDuration : m_DefaultBlendDuration;
            m_Animator.speed = 1f;


            if (duration > 0) {
                int hash = Animator.StringToHash(m_CurrentType.ToString());
                m_Animator.CrossFadeInFixedTime(hash, duration, 0);
            } else {
                m_Animator.Play(m_CurrentType.ToString(), 0, 0f);
            }
        }

        /// <summary>
        /// 停止：回到 Idle（带过渡）
        /// </summary>
        public void Stop()
        {
            m_CurrentType = m_DefaultAnimationType;
            Play(m_DefaultAnimationType, m_DefaultBlendDuration);
        }

        /// <summary>
        /// 立刻冻结在当前帧
        /// </summary>
        public void Pause()
        {
            if (!Valid()) {
                return;
            }
            m_Animator.speed = 0f;
        }

        /// <summary>
        /// 当前是否正在播放指定动画（可带前缀匹配，如 "Run" 匹配 "Run_Forward"）
        /// </summary>
        public bool IsPlaying(string stateName)
        {
            if (!Valid()) {
                return false;
            }
            var state = m_Animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName(stateName);
        }

        /// <summary>
        /// 获取当前主层动画的播放进度 [0, 1]
        /// </summary>
        public float GetProgress()
        {
            if (!Valid()) {
                return -1f;
            }
            var state = m_Animator.GetCurrentAnimatorStateInfo(0);
            return state.normalizedTime % 1f;
        }

        /// <summary>
        /// 当前是否在过渡（Blending）中
        /// </summary>
        public bool IsBlending()
        {
            if (!Valid()) {
                return false;
            }
            return m_Animator.IsInTransition(0);
        }

        /// <summary>
        /// 获取当前主层状态名
        /// </summary>
        public string GetCurrentStateName()
        {
            if (!Valid()) {
                return "";
            }
            return m_CurrentType.ToString();
        }

        private void InitializeOverride()
        {
            foreach(var entry in m_Clips) {
                if (entry.Clip != null) {
                    m_Override[entry.Type.ToString()] = entry.Clip;
                }
            }
        }
    }
}
