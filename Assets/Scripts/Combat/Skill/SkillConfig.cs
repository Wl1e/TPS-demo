using UnityEngine;

namespace TPSDemo
{
    [System.Serializable]
    public struct SkillArgs
    {
        public string Key;
        public string Value;
    };

    [CreateAssetMenu(menuName = "Config/Skill/SkillConfig", fileName = "SkillConfig")]
    public class SkillConfig: ScriptableObject
    {
        public string Name;
        [Tooltip("攻击范围")]
        public Vector2 AttackRange;
        [Tooltip("CD")]
        public float Cooldown;
        [Tooltip("前摇时间")]
        public float WindupTime;
        [Tooltip("持续时间")]
        public float ActiveTime;
        [Tooltip("后摇时间")]
        public float RecoveryTime;
        [Tooltip("伤害")]
        public float Damage;
        [Tooltip("对应动画")]
        public string AnimationName;

        // 目前skill很少，每一个skill一个config感觉没必要
        // 后续可以考虑继承config
        [Tooltip("额外参数")]
        public System.Collections.Generic.List<SkillArgs> Args;
    };
}
