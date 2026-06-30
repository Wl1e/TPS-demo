using UnityEngine;

namespace TPSDemo
{
    public class AudioBuilder
    {
        AudioSystem m_AudioSystem;
        AudioClip m_AudioClip;
        Transform m_AttachTransform;
        Vector3 m_Position;
        float m_Duration = float.PositiveInfinity;

        public AudioBuilder(AudioSystem audioSystem, AudioClip clip)
        {
            m_AudioSystem = audioSystem;
            m_AudioClip = clip;
        }

        public AudioBuilder AttachTo(Transform attach)
        {
            m_AttachTransform = attach;
            return this;
        }

        public AudioBuilder WithDuration(float time)
        {
            m_Duration = time;
            return this;
        }

        public AudioBuilder WithPosition(Vector3 position)
        {
            m_Position = position;
            return this;
        }

        public void Play()
        {
            if (m_AudioSystem == null || !m_AudioClip) {
                return;
            }
            m_AudioSystem.Play(new AudioSystem.AudioInfo {
                AudioClip = m_AudioClip,
                AttachTransform = m_AttachTransform,
                Position = m_Position,
                Duration = m_Duration
            });
        }
    }
}
