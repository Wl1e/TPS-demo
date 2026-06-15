using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo.UI
{
    /// <summary>
    /// 左侧任务列表容器 — 管理 QuestListItemUI 的生成、复用和高亮
    /// </summary>
    public class QuestListUI : MonoBehaviour
    {
        [Tooltip("任务列表单项预制体")]
        [SerializeField] QuestListItemUI m_ItemPrefab;
        [Tooltip("列表 Content 节点 (挂 Vertical Layout Group)")]
        [SerializeField] Transform m_Content;
        [Tooltip("选中项高亮颜色")]
        [SerializeField] Color m_SelectedColor = Color.yellow;
        [Tooltip("未选中项正常颜色")]
        [SerializeField] Color m_NormalColor = Color.white;

        System.Collections.Generic.List<QuestListItemUI> m_Items = new();
        public int SelectedIndex = -1;
        public Action<int> OnQuestSelected;

        public int CurrentQuestId => SelectedIndex != -1 ? m_Items[SelectedIndex].QuestId : -1;

        private void AddItem()
        {
            var item = Instantiate(m_ItemPrefab, m_Content);
            item.OnClick = OpQuestClick;
            m_Items.Add(item);
        }

        public void Refresh(NetworkList<QuestProcess> processes)
        {
            while (m_Items.Count < processes.Count) {
                AddItem();
            }
            while (m_Items.Count > processes.Count) {
                Destroy(m_Items[^1].gameObject);
                m_Items.RemoveAt(m_Items.Count - 1);
            }
            // 每次都刷有没有性能问题，或许可以考虑一个Quest一个QuestListItemUI
            for (int i = 0; i < processes.Count; i++) {
                var config = ResourceManager.Instance.GetResource<QuestDatabase>("Quest").GetQuestConfig(processes[i].Id);
                m_Items[i].SetName(config.QuestName, processes[i].Id);
            }
        }

        private void OpQuestClick(QuestListItemUI quest)
        {
            for (int idx = 0; idx < m_Items.Count; idx++) {
                if (m_Items[idx] == quest) {
                    SelectedIndex = idx;
                    m_Items[idx].SetHighlight(m_SelectedColor);
                } else {
                    m_Items[idx].SetHighlight(m_NormalColor);
                }
            }
            OnQuestSelected?.Invoke(SelectedIndex);
        }
    }
}

