using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TPSDemo
{
	public class AudioDef: MonoBehaviour
	{
        // 如果需要顺序播放一组音频，再考虑使用AudioClip数组
        //public List<AudioClip> AudioClips;
        public AudioClip AudioClip;
        public RangedFloat Loop;
        public float VolumeScale = 1f;
    }
}
