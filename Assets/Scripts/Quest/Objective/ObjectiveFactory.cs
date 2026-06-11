using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace TPSDemo
{
    public static class ObjectiveFactory
    {
        static public Dictionary<string, Objective> Objectives =
            new Dictionary<string, Objective> { };
        // FIXME
        static public Objective CreateObjective(ObjectiveConfig config)
        {
            Assert.IsNotNull(config);
            Objective objective = null;
            // 后续拆开
            if (config is ObjectiveKillEnemiesConfig) {
                objective = new ObjectiveKillEnemies();
                
            } else if (config is ObjectivePickupItemConfig) {
                objective = new ObjectivePickupItem();
            }
            if (objective != null) {
                objective.Id = config.Id;
                objective.Initialize(config);
            }
            return objective;
        }
    }
}
