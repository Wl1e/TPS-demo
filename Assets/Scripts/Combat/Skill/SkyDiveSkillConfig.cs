using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Config/Skill/SkyDiveSkillConfig", fileName = "SkyDiveSkillConfig")]
    public class SkyDiveSkillConfig: SkillConfig
	{
        [Header("坠落技能配置")]
        [Tooltip("升空速度")]
        public float RiseSpeed = 10f;
        [Tooltip("坠落速度")]
        public float DiveSpeed = 20f;
        [Tooltip("伤害半径")]
        public float AoeRadius = 1f;
        [Tooltip("碰撞层")]
        public LayerMask TargetLayer;
    }
}
