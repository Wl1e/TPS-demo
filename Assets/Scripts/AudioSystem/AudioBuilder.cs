using UnityEngine;
using System.Collections;

namespace TPSDemo
{
    public class AudioBuilder
    {
        AudioSystem m_AudioSystem;
        AudioDef m_AudioDef;
        Transform m_AttachTransform;
        Vector3 m_Position;

        public AudioBuilder(AudioSystem audioSystem, AudioDef def)
        {
            m_AudioSystem = audioSystem;
            m_AudioDef = def;
        }

        public AudioBuilder AttachTo(Transform attach)
        {
            m_AttachTransform = attach;
            return this;
        }

        public AudioBuilder WithPosition(Vector3 position)
        {
            m_Position = position;
            return this;
        }

        public void Play()
        {
            if (!m_AudioSystem || !m_AudioDef) {
                return;
            }
            m_AudioSystem.Player(new AudioSystem.AudioInfo {
                AudioDef = m_AudioDef,
                AttachTransform = m_AttachTransform,
                Position = m_Position
            });
        }
    }
}
