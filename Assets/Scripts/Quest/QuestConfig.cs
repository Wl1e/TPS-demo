using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(menuName = "Quest/QuestConfig", fileName = "QuestConfig")]
    public class QuestConfig: ScriptableObject
    {
        public int QuestId;
        public string QuestName;
        public string QuestDescription;
        public List<ObjectiveConfig> ObjectiveConfigs;
        public List<PairEntry<GameObject, int>> QuestReward;
    };

    [CreateAssetMenu(menuName = "Quest/QuestDatabase", fileName = "QuestDatabase")]
    public class QuestDatabase : ScriptableObject
    {
        public List<QuestConfig> QuestConfigs;
        private readonly Dictionary<int, QuestConfig> m_Lookups = new();

        public QuestConfig GetQuestConfig(int id)
        {
            QuestConfig config = null;
            if (m_Lookups.TryGetValue(id, out config)) {
                return config;
            }
            config = null;
            int idx = QuestConfigs.FindIndex(conf => conf.QuestId == id);
            if (idx != -1) {
                config = QuestConfigs[idx];
                m_Lookups[config.QuestId] = config;
            }
            return config;
        }

        public bool HasQuestConfig(int id)
        {
            return m_Lookups.ContainsKey(id) || QuestConfigs.Exists(config => config.QuestId == id);
        }
    }
}
