

using System;
using UnityEngine;

namespace TPSDemo
{
    [RequireComponent(typeof(AudioSource))]
	public class AudioPlayer: MonoBehaviour
	{
        AudioSource m_AudioSource;
        bool m_IsPlaying;
        float m_Duration;

        public Action<AudioPlayer> OnRelease;

        public AudioSource AudioSource => GetComponent<AudioSource>();

        public bool IsPlaying => m_IsPlaying;
        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_IsPlaying = false;
        }

        private void Update()
        {
            if (m_IsPlaying) {
                m_Duration -= Time.deltaTime;
                if(m_Duration <= 0f) {
                    m_IsPlaying = false;
                    Release();
                    return;
                }
                if (!m_AudioSource.isPlaying) {
                    m_IsPlaying = m_AudioSource.isPlaying;
                    Release();
                    return;
                }
            }
        }

        public void SetAudioGroup(UnityEngine.Audio.AudioMixer audioMixer)
        {
            //m_AudioSource.outputAudioMixerGroup = audioMixer;
        }
        public void Play(AudioClip clip, UnityEngine.Audio.AudioMixerGroup group, float duration)
        {
            m_IsPlaying = true;
            m_AudioSource.outputAudioMixerGroup = group;
            m_AudioSource.clip = clip;
            m_AudioSource.Play();
            m_Duration = duration;
        }

        public void Stop()
        {
            m_IsPlaying = false;
            m_AudioSource.Stop();
        }
        public void Release()
        {
            if (m_IsPlaying) {
                Stop();
            }
            if (m_AudioSource) {
                m_AudioSource.outputAudioMixerGroup = null;
            }
            OnRelease?.Invoke(this);
        }

        public void OnDestroy()
        {
            if (!gameObject.scene.isLoaded) {
                return;
            }
            OnRelease?.Invoke(this);
        }
    }
}
