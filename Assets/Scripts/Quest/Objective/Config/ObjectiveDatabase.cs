using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TPSDemo
{
    [CreateAssetMenu(fileName = "ObjectiveDatabase", menuName = "Config/ObjectiveConfig/ObjectiveDatabase")]
	public class ObjectiveDatabase: GameResource
	{
        public List<ObjectiveConfig> Configs;
        public ObjectiveConfig GetConfig(int id)
        {
            int idx = Configs.FindIndex(config => config.Id == id);
            return idx == -1 ? null : Configs[idx];
        }

	}
}
