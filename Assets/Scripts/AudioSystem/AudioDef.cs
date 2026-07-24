using System;
using UnityEngine;

namespace TPSDemo
{
    [Obsolete("暂时用不上，后续有更复杂的需求时再考虑扩展")]
    [CreateAssetMenu(fileName = "AudioDef", menuName = "Other/Sound/AudioDef")]
	public class AudioDef: ScriptableObject
    {
        // 如果需要顺序播放一组音频，再考虑使用AudioClip数组
        //public List<AudioClip> AudioClips;
        public AudioClip AudioClip;
        public RangedFloat Loop;
        public float VolumeScale = 1f;
    }
}
