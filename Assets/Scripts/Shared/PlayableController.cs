using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace TPSDemo
{
    [RequireComponent(typeof(Animator))]
    public class PlayableController : MonoBehaviour
    {
        [Tooltip("动画")]
        public List<AnimationEntry> Clips;
        [Tooltip("混合时间")]
        [SerializeField] private float m_FadeDuration = 0.2f;
        [Tooltip("默认动画")]
        [SerializeField] string m_DefaultAnimationName = "Idle";

        private int DefaultAnimationIndex => m_ClipsIndex.GetValueOrDefault(m_DefaultAnimationName, -1);

        private readonly Dictionary<string, int> m_ClipsIndex = new();
        private readonly List<AnimationClipPlayable> m_ClipPlayables = new();
        private PlayableGraph m_Graph;
        private AnimationMixerPlayable m_Mixer;
        private AnimationPlayableOutput m_Output;

        private Coroutine m_TransitionCoroutine = null;
        private int m_CurrentClipIdx = -1;
        private int m_PreviousClipIdx = -1;

        public event Action<int, int, float> OnAnimationPlay;
        public event Action<int, float> OnAnimationPause;

        public bool IsPlaying()
        {
            if (m_CurrentClipIdx == -1 || m_CurrentClipIdx == DefaultAnimationIndex) {
                return false;
            }
            var playable = m_ClipPlayables[m_CurrentClipIdx];
            return (playable.GetAnimationClip().isLooping || playable.GetTime() < playable.GetDuration());
        }

        public string CurrentClipName => ((AnimationClipPlayable)m_Mixer.GetInput(m_CurrentClipIdx)).GetAnimationClip().name;
        public bool HasClip(string name)
        {
            if (!m_ClipsIndex.TryGetValue(name, out var idx)) {
                return false;
            }
            return idx >= 0 && idx < m_ClipPlayables.Count;
        }

        void Awake()
        {
            var animator = GetComponent<Animator>();
            animator.applyRootMotion = false;       // ← 这行
            m_Graph = PlayableGraph.Create(gameObject.name);
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            m_Mixer = AnimationMixerPlayable.Create(m_Graph, Clips.Count);

            for(int  i = 0; i < Clips.Count; i++) {
                var name = Clips[i].Name;
                var clip = Clips[i].Clip;

                var playable = AnimationClipPlayable.Create(m_Graph, clip);
                playable.SetDuration(clip.length);

                m_ClipsIndex[name] = i;
                m_ClipPlayables.Add(playable);
                playable.SetTime(0);
                playable.Pause();
                m_Mixer.ConnectInput(i, m_ClipPlayables[i], 0, 0);
            }

            m_Output = AnimationPlayableOutput.Create(m_Graph, "Output", animator);
            m_Output.SetSourcePlayable(m_Mixer);

            m_Graph.Play();

            m_Mixer.SetInputWeight(DefaultAnimationIndex, 1);
            Play(m_DefaultAnimationName);
        }

        void OnDestroy()
        {
            if (m_Graph.IsValid()) {
                m_Graph.Destroy();
            }
        }

        void OnDisable()
        {
            if (m_Graph.IsValid()) {
                m_Graph.Stop();
            }
        }

        void OnEnable()
        {
            if (m_Graph.IsValid()) {
                m_Graph.Play();
            }
        }

        void Reset()
        {
            GetComponent<Animator>().runtimeAnimatorController = null;
        }

        private void Update()
        {   
            if(m_CurrentClipIdx != DefaultAnimationIndex && !IsPlaying()) {
                Stop();
            }
        }

        [Obsolete("目前只能固定动画数量，如需动态增删，需实现mixer的重构")]
        public void RegisterAnimationClip(string name, AnimationClip clip)
        {
            if (HasClip(name)) {
                m_ClipPlayables[m_ClipsIndex[name]].Destroy();
            }

            var idx = m_ClipsIndex[name];
            var playable = AnimationClipPlayable.Create(m_Graph, clip);
            playable.SetDuration(clip.length);
            m_ClipPlayables[idx] = playable;
        }

        [Obsolete("目前只能固定动画数量，如需动态增删，需实现mixer的重构")]
        public void UnregisterAnimationClip(string name)
        {
            //if (!m_ClipPlayables.TryGetValue(name, out var playable)) {
            //    return;
            //}

            //if (m_CurrentClipName == name) {
            //    Pause();
            //}

            //playable.Destroy();
            //m_ClipPlayables.Remove(name);
        }

        // Client use
        public void Play(int pre, int cur, float fadeDuration)
        {
            //Pause(0);
            // 或者应该断言这两个变量必须相同，或者没意义
            m_CurrentClipIdx = pre;
            PlayAnimation(cur, fadeDuration);
        }

        // Owner use
        public void Play(string name, float fadeDuration = -1f)
        {
            print("Play " + name);
            if (!HasClip(name)) {
                Debug.LogWarning($"Animation clip '{name}' not registered.");
                return;
            }

            PlayAnimation(m_ClipsIndex[name], fadeDuration);
        }

        /// <summary>
        /// 更新m_PreviousClipIdx和m_CurrentClipIdx，可打断m_TransitionCoroutine
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="fadeDuration"></param>
        private void PlayAnimation(int idx, float fadeDuration)
        {
            if (m_CurrentClipIdx == idx) {
                m_ClipPlayables[idx].SetTime(0);
                m_ClipPlayables[idx].Play();
                return;
            }

            if (fadeDuration == -1f) {
                fadeDuration = m_FadeDuration;
            }

            // 打断正在进行的过渡
            if(m_TransitionCoroutine != null) {
                StopAllAnimation();
                StopCoroutine(m_TransitionCoroutine);
                m_TransitionCoroutine = null;
            }

            m_PreviousClipIdx = m_CurrentClipIdx;
            m_CurrentClipIdx = idx;

            var playable = m_ClipPlayables[m_CurrentClipIdx];
            playable.SetTime(0f);
            playable.Play();
            
            if (fadeDuration <= 0f) {
                if (m_PreviousClipIdx != -1) {
                    m_ClipPlayables[m_PreviousClipIdx].Pause();
                    m_Mixer.SetInputWeight(m_PreviousClipIdx, 0f);
                }
                m_Mixer.SetInputWeight(m_CurrentClipIdx, 1f);
            } else {
                m_TransitionCoroutine = StartCoroutine(
                    FadeTransition(m_PreviousClipIdx, m_CurrentClipIdx, fadeDuration)
                );
            }
            OnAnimationPlay?.Invoke(m_PreviousClipIdx, m_CurrentClipIdx, fadeDuration);
        }

        /// <summary>
        /// 停止idx动画并清空权重
        /// </summary>
        /// <param name="idx"></param>
        private void StopAnimation(int idx)
        {
            if(idx < 0 || idx > m_ClipPlayables.Count) {
                return;
            }
            m_ClipPlayables[idx].Pause();
            m_ClipPlayables[idx].SetTime(0f);
            m_Mixer.SetInputWeight(idx, 0f);
        }

        /// <summary>
        /// 停止所有动画播放并清空权重
        /// </summary>
        private void StopAllAnimation()
        {
            for (int i = 0; i < m_ClipPlayables.Count; i++) {
                m_ClipPlayables[i].Pause();
                m_ClipPlayables[i].SetTime(0f);
                m_Mixer.SetInputWeight(i, 0);
            }
        }

        public void Pause(float fadeDuration = -1f)
        {
            if(fadeDuration == -1f) {
                fadeDuration = m_FadeDuration;
            }
            if (m_TransitionCoroutine != null) {
                StopCoroutine(m_TransitionCoroutine);
                m_TransitionCoroutine = null;
            }

            m_PreviousClipIdx = m_CurrentClipIdx;
            m_CurrentClipIdx = -1;

            if (fadeDuration <= 0f) {
                if (m_PreviousClipIdx != -1) {
                    m_ClipPlayables[m_PreviousClipIdx].Pause();
                }

                foreach (var idx in m_ClipsIndex.Values) {
                    m_Mixer.SetInputWeight(idx, 0f);
                }
            } else {
                m_TransitionCoroutine = StartCoroutine(FadeStop(fadeDuration));
            }
            OnAnimationPause?.Invoke(m_PreviousClipIdx, fadeDuration);
        }

        // Owner use
        public void Stop(float fadeDuration = -1f) => Play(m_DefaultAnimationName, fadeDuration);

        /// <summary>
        /// 动画过渡
        /// </summary>
        /// <param name="fromPort"> 过渡前 </param>
        /// <param name="toPort"> 过渡后 </param>
        /// <param name="duration"> 持续时间 </param>
        /// <returns></returns>
        System.Collections.IEnumerator FadeTransition(int fromPort, int toPort, float duration)
        {
            float time = 0f;

            print($"{fromPort} => {toPort}: {duration}");

            m_Mixer.SetInputWeight(toPort, 0f);
            if (fromPort >= 0) {
                m_Mixer.SetInputWeight(fromPort, 1f);

                while (time < duration) {
                    time += Time.deltaTime;
                    float t = Mathf.Clamp01(time / duration);
                    m_Mixer.SetInputWeight(fromPort, 1f - t);
                    m_Mixer.SetInputWeight(toPort, t);
                    yield return null;
                }
                StopAnimation(fromPort);
            }
            else {
                while (time < duration) {
                    time += Time.deltaTime;
                    float t = Mathf.Clamp01(time / duration);
                    m_Mixer.SetInputWeight(toPort, t);
                    yield return null;
                }
            }
            
            m_Mixer.SetInputWeight(toPort, 1f);
            m_TransitionCoroutine = null;
        }

        /// <summary>
        /// 淡出当前动画
        /// </summary>
        /// <param name="duration"> 持续时间 </param>
        /// <returns></returns>
        System.Collections.IEnumerator FadeStop(float duration)
        {
            // 当前没有播放动画
            if(m_CurrentClipIdx == -1) {
                yield break;
            }

            float time = 0f;
            // 如果正在动画过渡，则一起淡出?
            float w0 = m_PreviousClipIdx != -1 ? m_Mixer.GetInputWeight(m_PreviousClipIdx) : 0f;
            float w1 = m_Mixer.GetInputWeight(m_CurrentClipIdx);

            while (time < duration) {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                if (m_PreviousClipIdx != -1 && w0 > 0f) {
                    m_Mixer.SetInputWeight(m_PreviousClipIdx, Mathf.Lerp(w0, 0f, t));
                }
                m_Mixer.SetInputWeight(m_CurrentClipIdx, Mathf.Lerp(w1, 0f, t));
                yield return null;
            }

            m_Mixer.SetInputWeight(m_PreviousClipIdx, 0f);
            m_Mixer.SetInputWeight(m_CurrentClipIdx, 0f);

            if (m_PreviousClipIdx != -1) {
                m_ClipPlayables[m_PreviousClipIdx].Pause();
            }
            m_ClipPlayables[m_CurrentClipIdx].Pause();

            m_TransitionCoroutine = null;
        }
    }
}
