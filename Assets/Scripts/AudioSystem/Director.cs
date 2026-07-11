using UnityEngine;
using UnityEngine.Audio;

namespace TPSDemo
{
    using System;
    using UI;
	public class Director: Singleton<Director>
	{
        [SerializeField] private AudioMixer m_Mixer;
        private AudioSystem m_AudioSystem;
        private EffectPool m_EffectPool;
        [SerializeField] private DamageValueUI m_DVPrefab;
        private DamageValuePool m_DamageValuePool;

        bool m_SoundMuted = false;
        // Use this for initialization

        protected override void Awake()
        {
            base.Awake();
            m_AudioSystem = new AudioSystem();
            m_AudioSystem.Initialize();
            m_EffectPool = new EffectPool();
            m_EffectPool.Initialize();
            m_DamageValuePool = new DamageValuePool();
            m_DamageValuePool.Initialize(m_DVPrefab);
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

        public EffectBuilder RequestEffect(UnityEngine.AddressableAssets.AssetReference effectPrefab)
        {
            return m_EffectPool.GetEffectBuilder().SetEffect(effectPrefab);
        }

        public void RequestDamageValue(Vector3 position, float damageValue, bool isCritical) => m_DamageValuePool.ShowDV(position, damageValue, isCritical);
    }
}
