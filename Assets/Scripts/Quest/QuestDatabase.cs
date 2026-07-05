using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Config/Quest/QuestDatabase", fileName = "QuestDatabase")]
    public class QuestDatabase: GameResource
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

