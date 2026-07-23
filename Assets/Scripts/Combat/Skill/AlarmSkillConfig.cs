using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Config/Skill/AlarmSkillConfig", fileName = "AlarmSkillConfig")]
    public class AlarmSkillConfig: SkillConfig
    {
        [Header("警报技能配置")]
        [Tooltip("警报生效范围")] 
        public float AlarmRange = 3f;
        [Tooltip("警报持续时间")]
        public float AlarmTime = 3f;
        [Tooltip("警报音效")]
        public AudioClip AlarmClip;
    };
}
