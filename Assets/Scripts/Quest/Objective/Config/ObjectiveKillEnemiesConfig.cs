using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(menuName = "Config/ObjectiveConfig/ObjectiveKillEnemiesConfig", fileName = "ObjectiveKillEnemiesConfig")]
    public class ObjectiveKillEnemiesConfig : ObjectiveConfig
    {
        // FIXME: 后续改为ID
        public GameObject Target;
        public int Count;
        public override string GetObjectiveText() => $"击败 {Count}个 {Target.name}";
    };
}
