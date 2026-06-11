using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


namespace TPSDemo
{

    public class QuestController: NetworkSingleton<QuestController>
    {
        [SerializeField] private QuestDatabase m_QuestDatabase;
        // Server owner
        private readonly Dictionary<int, Quest> m_Quests = new();
        // Server read and write, Client read
        private readonly NetworkList<QuestProcess> m_Processes = new();
        public NetworkList<QuestProcess> QuestProcesses => m_Processes;

        public override void OnNetworkSpawn()
        {
            if(IsClient) {
                m_Processes.OnListChanged += OnQuestProcessChanged;
            }
        }

        private QuestProcess GetProcess(int QuestId)
        {
            foreach (QuestProcess process in m_Processes) {
                if (process.Id == QuestId) {
                    return process;
                }
            }
            return default;
        }

        #region Client

        public bool HasTask(int questId) => m_Quests.ContainsKey(questId);

        public void RemoveTask(int questId)
        {
            RemoveQuestServerRpc(questId);
        }

        

        public void AddQuest(int questId)
        {
            AddQuestServerRpc(questId);
        }

        //[ClientRpc]
        //private void OnQuestAddedClientRpc()

        private void OnQuestProcessChanged(NetworkListEvent<QuestProcess> changeEvent)
        {
            UpdateUI();
        }

        private void QuestCheck(Quest quest)
        { }

        #endregion

        #region Server

        QuestProcess CreateProcess(Quest quest)
        {
            var process = new QuestProcess {
                Id = quest.QuestId,
                State = QuestState.None
            };

            quest.UpdateProcess(ref process);
            return process;
        }

        [ServerRpc]
        private void AddQuestServerRpc(int questId)
        {
            if (m_Quests.ContainsKey(questId)) {
                return;
            }
            var config = m_QuestDatabase.GetQuestConfig(questId);
            if (!config) {
                return;
            }
            var quest = QuestFactory.CreateQuest(config);
            m_Quests[questId] = quest;
            m_Processes.Add(CreateProcess(quest));
            quest.OnQuestCompleted += OnQuestCompleted;
            quest.OnQuestUpdate += OnQuestUpdate;
        }

        [ServerRpc]
        private void RemoveQuestServerRpc(int questId)
        {
            if (!m_Quests.ContainsKey(questId)) {
                return;
            }
            m_Quests.Remove(questId, out var quest);
            foreach(var process in m_Processes) {
                if (process.Id == questId) {
                    m_Processes.Remove(process);
                    break;
                }
            }
            quest.OnQuestCompleted -= OnQuestCompleted;
            quest.OnQuestUpdate -= OnQuestUpdate;
            quest.Destroy();
        }

        private void OnQuestCompleted(Quest quest)
        {
            Reward();
            UpdateUI();
        }

        private void OnQuestUpdate(Quest quest)
        {
            UpdateUI();
        }

        // 改成玩家点击领取?
        private void Reward()
        { }

        //[ServerRpc]
        //private void QuestCheckServerRpc(int questId)
        //{
        //    if (m_Quests.TryGetValue(questId, out var quest)) {
        //        quest.UpdateTask();
        //    }
        //}

        #endregion

        #region UI

        private void UpdateUI()
        {
            EventManager.Broadcast(new Event.QuestUpdateEvent());
        }

        #endregion
    }
}
