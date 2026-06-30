using UnityEngine;
using UnityEngine.Audio;

namespace TPSDemo
{
	public class Director: Singleton<Director>
	{
        [SerializeField] AudioMixer m_Mixer;
        [SerializeField] Transform EffectPool;
        AudioSystem m_AudioSystem;
        EffectPool m_EffectPool;

        bool m_SoundMuted = false;
        // Use this for initialization

        protected override void Awake()
        {
            base.Awake();
            m_AudioSystem = new AudioSystem();
            m_AudioSystem.Initialize();
            m_EffectPool = new EffectPool();
            m_EffectPool.Initialize();
        }

        public AudioBuilder RequestAudio(AudioClip clip)
        {
            return new AudioBuilder(m_AudioSystem, clip);
        }

        // 用完记得Release
        public AudioPlayer Borrow() => m_AudioSystem.Borrow();

        public void MuteSound(bool mute)
        {
            m_SoundMuted = mute;
            m_Mixer.SetFloat("Volume", mute ? -80f : 0f);
        }
        public EffectBuilder RequestEffect(GameObject effectPrefab)
        {
            return m_EffectPool.GetEffectBuilder().SetEffect(effectPrefab);
        }
    }
}
