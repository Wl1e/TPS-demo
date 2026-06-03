using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

namespace TPSDemo
{
	public class Director: Singleton<Director>
	{
        [SerializeField] AudioMixer m_Mixer;
        [SerializeField] Transform EffectPool;
        AudioSystem m_AudioSystem;

        bool m_SoundMuted = false;
        // Use this for initialization

        protected override void Awake()
        {
            base.Awake();
            m_AudioSystem = new AudioSystem();
            m_AudioSystem.Initialize();
        }

        public AudioBuilder RequestAudio(AudioClip clip)
        {
            return new AudioBuilder(m_AudioSystem, clip);
        }

        public void MuteSound(bool mute)
        {
            m_SoundMuted = mute;
            m_Mixer.SetFloat("Volume", mute ? -80f : 0f);
        }
        public EffectBuilder RequestEffect(GameObject effectPrefab)
        {
            var effectBuilder = new EffectBuilder(effectPrefab);
            return effectBuilder;
        }
    }
}
