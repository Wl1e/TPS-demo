
using UnityEngine;
using UnityEngine.Assertions;

namespace TPSDemo
{

    public static class QuestFactory    {
        static public Quest CreateQuest(QuestConfig config)
        {
            Assert.IsNotNull(config);
            Quest quest = new();
            quest.Initialize(config.QuestId);
            foreach (var objectiveConfig in config.ObjectiveConfigs) {
                var objective = ObjectiveFactory.CreateObjective(objectiveConfig);
                quest.AddObjective(objective);
            }
            return quest;
        }
    };
}
