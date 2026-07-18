using UnityEngine;
using UnityEngine.Audio;

namespace TPSDemo
{
    public class AudioBuilder
    {
        AudioSystem m_AudioSystem;
        AudioClip m_AudioClip;
        readonly UnityEngine.AddressableAssets.AssetReference m_AudioRef;
        Transform m_AttachTransform;
        Vector3 m_Position;
        float m_Duration = float.PositiveInfinity;
        AudioMixerGroup m_MixerGroup;

        public AudioBuilder(AudioSystem audioSystem, AudioClip clip)
        {
            m_AudioSystem = audioSystem;
            m_AudioClip = clip;
        }

        public AudioBuilder(AudioSystem audioSystem, UnityEngine.AddressableAssets.AssetReference audioRef)
        {
            m_AudioSystem = audioSystem;
            m_AudioRef = audioRef;
        }


        public AudioBuilder WithMixerGroup(AudioMixerGroup group)
        {
            m_MixerGroup = group;
            return this;
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
            if (m_AudioSystem == null) {
                return;
            }
            if (m_AudioClip != null) {
                m_AudioSystem.Play(
                    new AudioSystem.AudioInfo {
                        AudioClip = m_AudioClip,
                        AttachTransform = m_AttachTransform,
                        Position = m_Position,
                        Duration = m_Duration
                    }
                );
            }
        }
    }
}
