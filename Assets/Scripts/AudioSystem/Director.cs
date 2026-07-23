using UnityEngine;
using UnityEngine.Audio;

namespace TPSDemo
{
    using UI;
	public class Director: Singleton<Director>
	{
        private AudioSystem m_AudioSystem;
        private EffectPool m_EffectPool;
        [SerializeField] private DamageValueUI m_DVPrefab;
        private DamageValuePool m_DamageValuePool;

        [SerializeField] private AudioMixer m_Mixer;
        [SerializeField] private AudioMixerGroup m_MasterGroup;
        [SerializeField] private AudioMixerGroup m_BGMGroup;
        [SerializeField] private AudioMixerGroup m_SFXGroup;

        // Use this for initialization

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx">
        /// 0: Master
        /// 1: BGM
        /// 2: SFX
        /// </param>
        /// <returns></returns>
        public AudioMixerGroup GetGroup(AudioSystem.AudioGroup idx) => idx switch {
            AudioSystem.AudioGroup.Master => m_MasterGroup,
            AudioSystem.AudioGroup.BGM => m_BGMGroup,
            AudioSystem.AudioGroup.SFX => m_SFXGroup,
            _ => null
        };

        protected override void Awake()
        {
            base.Awake();
            m_AudioSystem = new AudioSystem();
            m_AudioSystem.Initialize(m_Mixer);
            m_EffectPool = new EffectPool();
            m_EffectPool.Initialize();
            m_DamageValuePool = new DamageValuePool();
            m_DamageValuePool.Initialize(m_DVPrefab);
        }

        public AudioBuilder RequestAudio(AudioClip clip)
        {
            return new AudioBuilder(m_AudioSystem, clip);
        }
        public AudioBuilder RequestAudio(UnityEngine.AddressableAssets.AssetReference clip)
        {
            return new AudioBuilder(m_AudioSystem, clip);
        }

        // 用完记得Release
        public AudioPlayer Borrow() => m_AudioSystem.Borrow();

        public void SetVolume(int idx, float volume)
        {
            if (idx == 0) {
                m_Mixer.SetFloat("Master", volume);
            } else if (idx == 1) {
                m_Mixer.SetFloat("BGM", volume);
            } else if (idx == 2) {
                m_Mixer.SetFloat("SFX", volume);
            }
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
