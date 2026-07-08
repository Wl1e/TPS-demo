using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Config/Skill/SkillDatabase", fileName = "SkillDatabase")]
    public class SkillDatabase: GameResource
    {
        public System.Collections.Generic.List<SkillConfig> Skills;

        public SkillConfig GetConfig(string name)
        {
            int idx = Skills.FindIndex(config => config.Name == name);
            return idx != -1 ? Skills[idx] : null;
        }
    };
}
