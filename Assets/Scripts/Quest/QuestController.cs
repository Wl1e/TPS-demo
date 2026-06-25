using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


namespace TPSDemo
{

    public class QuestController: NetworkBehaviour
    {
        Actor m_Actor;
        private QuestDatabase m_QuestDatabase => ResourceManager.Instance.GetResource<QuestDatabase>("Quest");
        // Server owner
        private readonly Dictionary<int, Quest> m_Quests = new();
        // Server read and write, Client read
        private readonly NetworkList<QuestProcess> m_Processes = new();
        public NetworkList<QuestProcess> QuestProcesses => m_Processes;

        private bool m_UIOpened =false;
        [SerializeField] GameEvent m_OpenQuestPanelInput;

        PlayerController m_PlayerController;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_PlayerController = GetComponent<PlayerController>();
            if (IsClient) {
                m_Processes.OnListChanged += OnQuestProcessChanged;
            }
            if(IsOwner) {
                EventManager.AddListener<Event.TryQuestRewardEvent>(TryReward);
                EventManager.AddListener<Event.TryCancelQuestEvent>(TryCancelQuest);
                m_OpenQuestPanelInput.RegisterListener(OpenQuestPanel);
            }
            m_Actor = GetComponent<Actor>();
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient) {
                m_Processes.OnListChanged -= OnQuestProcessChanged;
            }
            if (IsOwner) {
                m_OpenQuestPanelInput.UnregisterListener(OpenQuestPanel);
            }
            base.OnNetworkDespawn();
        }

        private int GetProcessIdx(int QuestId)
        {
            for (int i = 0; i < m_Processes.Count; i++) {
                if (m_Processes[i].Id == QuestId) {
                    return i;
                }
            }
            return -1;
        }

        #region Client

        public bool HasQuest(int questId) => m_Quests.ContainsKey(questId);

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

        private void TryReward(Event.TryQuestRewardEvent evt)
        {
            var id = evt.QuestId;
            var idx = GetProcessIdx(id);
            if(idx == -1) {
                return;
            }
            if (m_Processes[idx].State != QuestState.Succeeded) {
                return;
            }
            TryRewardServerRpc(id);
        }
        private void TryCancelQuest(Event.TryCancelQuestEvent evt)
        {
            RemoveQuestServerRpc(evt.QuestId);
        }

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
            quest.SetActor(m_Actor.Id);
            m_Processes.Add(CreateProcess(quest));
            //quest.OnQuestCompleted += OnQuestCompleted;
            quest.OnQuestUpdate += OnQuestUpdate;
            quest.OnQuestCompleted += OnQuestUpdate;
        }

        [ServerRpc]
        private void RemoveQuestServerRpc(int questId)
        {
            RemoveQuestServer(questId);
        }

        private void RemoveQuestServer(int questId)
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
            //quest.OnQuestCompleted -= OnQuestCompleted;
            quest.OnQuestUpdate -= OnQuestUpdate;
            quest.OnQuestCompleted -= OnQuestUpdate;
            quest.Destroy();
        }

        private void OnQuestCompleted(Quest quest)
        {
            TryReward(quest);
        }

        private void OnQuestUpdate(Quest quest)
        {
            var processIdx = GetProcessIdx(quest.QuestId);
            if(processIdx != -1) {
                var process = m_Processes[processIdx];
                quest.UpdateProcess(ref process);
                m_Processes[processIdx] = process;
            }
        }

        // 改成玩家点击领取?
        private void TryReward(Quest quest)
        { }

        [ServerRpc]
        private void TryRewardServerRpc(int questId)
        {
            if(!HasQuest(questId)) {
                return;
            }
            var quest = m_Quests[questId];
            if(quest.IsReward) {
                return;
            }
            quest.FinishReward();

            var reward = m_QuestDatabase.GetQuestConfig(questId).QuestReward;
            foreach (var rew in reward) {
                if(rew.Key) {
                    m_PlayerController.Inventory.AddItem(rew.Key.Id, rew.Value);
                }
            }
            RemoveQuestServer(questId);
        }

        #endregion

        #region UI

        private void UpdateUI()
        {
            EventManager.Broadcast(new Event.QuestUpdateEvent());
        }

        private void OpenQuestPanel()
        {
            m_UIOpened = !m_UIOpened;
            if (m_UIOpened) {
                m_PlayerController.SetInputActive(false, false);
            } else {
                m_PlayerController.SetInputActive(true, true);
            }
            EventManager.Broadcast(new Event.QuestStateChangeEvent{ IsOpened = m_UIOpened });
        }

        #endregion
    }
}
