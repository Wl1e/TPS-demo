using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(menuName = "Config/Quest/QuestConfig", fileName = "QuestConfig")]
    public class QuestConfig: ScriptableObject
    {
        public int QuestId;
        public string QuestName;
        public string QuestDescription;
        public List<ObjectiveConfig> ObjectiveConfigs;
        public List<PairEntry<ItemData, int>> QuestReward;
    };
}
