using System.Diagnostics;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    /// <summary>
    /// 任务面板主控 — 监听 QuestUpdateEvent，刷新左侧列表和右侧详情
    /// </summary>
    public class QuestPanelUI : MonoBehaviour
    {
        [Header("左侧列表")]
        [Tooltip("任务列表容器组件")]
        [SerializeField] QuestListUI m_QuestList;

        [Header("右侧详情")]
        [Tooltip("任务标题文本")]
        [SerializeField] TextMeshProUGUI m_TitleText;
        [Tooltip("任务描述文本")]
        [SerializeField] TextMeshProUGUI m_DescriptionText;
        [Tooltip("任务目标列表根节点")]
        [SerializeField] Transform m_ObjectiveRoot;
        [Tooltip("任务目标单项预制体")]
        [SerializeField] ObjectiveProcessUI m_ObjectivePrefab;
        [Tooltip("领取奖励按钮")]
        [SerializeField] Button m_RewardButton;
        [Tooltip("取消任务按钮")]
        [SerializeField] Button m_CancelButton;

        [Header("数据")]
        [Tooltip("任务数据库 ScriptableObject")]
        [SerializeField] QuestDatabase m_QuestDB;
        [Tooltip("目标数据库 ScriptableObject")]
        ObjectiveDatabase m_ObjectiveDB => ResourceManager.Instance.GetResource<ObjectiveDatabase>("Objective");

        private bool m_IsOpened = false;

        System.Collections.Generic.List<ObjectiveProcessUI> m_ActiveObjectives = new();

        private void Start()
        {
            EventManager.AddListener<Event.QuestStateChangeEvent>(SetQuestState);
            EventManager.AddListener<Event.QuestUpdateEvent>(Refresh);

            m_QuestList.OnQuestSelected += UpdateQuestDetail;
            m_RewardButton.onClick.AddListener(
                () => EventManager.Broadcast(
                    new Event.TryQuestRewardEvent {
                        QuestId = m_QuestList.CurrentQuestId
                    }
                )
            );
            m_CancelButton.onClick.AddListener(
                () => {
                    EventManager.Broadcast(
                          new Event.TryCancelQuestEvent {
                              QuestId = m_QuestList.CurrentQuestId
                          }
                        );
                    m_QuestList.SelectedIndex = -1;
                    }
            );
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<Event.QuestStateChangeEvent>(SetQuestState);
            EventManager.RemoveListener<Event.QuestUpdateEvent>(Refresh);
        }

        public void Initialize() => Refresh();

        private void Refresh(Event.QuestUpdateEvent evt) => Refresh();

        private void Refresh()
        {
            var list = PlayerDataProxy.Instance.GetQuestProcesses();
            m_QuestList.Refresh(list);
            if (m_QuestList.SelectedIndex != -1) {
                UpdateQuestDetail(m_QuestList.SelectedIndex);
            }
        }

        private void SetQuestState(Event.QuestStateChangeEvent evt)
        {
            m_IsOpened = evt.IsOpened;
            if (m_IsOpened) {
                gameObject.SetActive(true);
                Refresh();
            } else {
                gameObject.SetActive(false);
            }
        }

        private bool HasQuest(NetworkList<QuestProcess> list, int id)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].Id == id) return true;
            return false;
        }

        private void ClearQuestDetail()
        {
            m_TitleText.text = "";
            m_DescriptionText.text = "";
            m_RewardButton.gameObject.SetActive(false);
            m_CancelButton.gameObject.SetActive(false);
            for (int i = 0; i < m_ActiveObjectives.Count; i++) {
                m_ActiveObjectives[i].gameObject.SetActive(false);
            }
        }

        private void UpdateQuestDetail(int idx)
        {
            if(idx == -1) {
                ClearQuestDetail();
                return;
            }
            var process = PlayerDataProxy.Instance.GetQuestProcesses()[idx];
            var cfg = m_QuestDB.GetQuestConfig(process.Id);
            if (cfg == null) {
                ClearQuestDetail();
                return;
            }

            m_TitleText.text = cfg.QuestName;
            m_DescriptionText.text = cfg.QuestDescription;

            var objs = new[] { process.Obj0, process.Obj1, process.Obj2, process.Obj3 };
            int count = 0;
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].IsEmpty) {
                    continue;
                }
                if (m_ActiveObjectives.Count <= count) {
                    m_ActiveObjectives.Add(Instantiate(m_ObjectivePrefab, m_ObjectiveRoot));
                }
                var ui = m_ActiveObjectives[count];
                ui.gameObject.SetActive(true);
                var oCfg = m_ObjectiveDB.GetConfig(objs[i].ObjectiveId);
                ui.Text.text = oCfg.GetObjectiveText();
                ui.Process.text = $"{objs[i].Cur} / {objs[i].Max}";
                count++;
            }

            // 按钮显现
            UpdateButton(process.State);

            // 隐藏不需要的Objective槽位
            for (int i = count; i < m_ActiveObjectives.Count; i++) {
                m_ActiveObjectives[i].gameObject.SetActive(false);
            }
        }

        private void UpdateButton(QuestState state)
        {
            if (state == QuestState.Succeeded) {
                m_RewardButton.gameObject.SetActive(true);
            } else {
                m_RewardButton.gameObject.SetActive(false);
            }
            if (state == QuestState.Succeeded || state == QuestState.Rewarded) {
                m_CancelButton.gameObject.SetActive(false);
            } else {
                m_CancelButton.gameObject.SetActive(true);
            }
        }
    }
}
