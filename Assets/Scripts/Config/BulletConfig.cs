using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(menuName = "Config/BulletConfig", fileName = "BulletConfig")]
    public class BulletConfig : ScriptableObject
    {
        [System.Serializable]
        public struct TagEffect
        {
            public string Tag;
            public GameObject Effect;
        }

        [Tooltip("最大存活时间")]
        public float MaxLifeTime = 5f;
        [Tooltip("攻击后直接销毁")]
        public bool DestroyOnHit = true;

        [Tooltip("冲击特效")]
        public List<TagEffect> HitImpactPrefab;

        [Tooltip("特效持续时间")]
        public float HitImpactDuration = 0.2f;

        [Tooltip("子弹孔")]
        public GameObject BulletHolePrefab;

        [Tooltip("子弹孔持续时间")]
        public float BulletHoleDuration = 2f;

        [Tooltip("攻击音效")]
        public AudioClip HitSfx;

        [Tooltip("音效持续时间")]
        public float HitSfxDuration = 0.1f;
    }

}
