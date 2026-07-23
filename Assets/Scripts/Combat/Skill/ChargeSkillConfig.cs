using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.STP;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Config/Skill/ChargeSkillConfig", fileName = "ChargeSkillConfig")]
	public class ChargeSkillConfig: SkillConfig
	{
        [Header("冲锋技能配置")]
        [Tooltip("冲锋速度")]
        public float ChargeSpeed;
        [Tooltip("冲锋门槛(在这个距离内视为抵达目标)")]
        public float Threshold;
    }
}
