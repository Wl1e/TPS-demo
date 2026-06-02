using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

namespace TPSDemo
{
	public class Director: Singleton<Director>
	{
        [SerializeField] AudioMixer m_Mixer;
        [SerializeField] Transform EffectPool;

        bool m_SoundMuted = false;
		// Use this for initialization
		void PlaySound(AudioDef def, Vector3 pos)
        {
        }
        public void MuteSound(bool mute)
        {
            m_SoundMuted = mute;
            m_Mixer.SetFloat("Volume", mute ? -80f : 0f);
        }
        public GameObject SpawnEffect(GameObject effectPrefab, Vector3 pos, Quaternion rot)
        {
            GameObject effect = Instantiate(effectPrefab, pos, rot, EffectPool.transform);
            return effect;
        }
    }
}
