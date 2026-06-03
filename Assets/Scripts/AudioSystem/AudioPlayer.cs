

using System;
using UnityEngine;

namespace TPSDemo
{
    [RequireComponent(typeof(AudioSource))]
	public class AudioPlayer: MonoBehaviour
	{
        AudioSource m_AudioSource;
        bool m_IsPlaying;

        public Action<AudioPlayer> OnRelease;
        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_IsPlaying = false;
        }

        private void Update()
        {
            if (m_IsPlaying && !m_AudioSource.isPlaying) {
                m_IsPlaying = m_AudioSource.isPlaying;
                Release();
            }
        }
        public void Play(AudioClip clip)
        {
            m_IsPlaying = true;
            m_AudioSource.clip = clip;
            m_AudioSource.Play();
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
            OnRelease?.Invoke(this);
        }
	}
}
