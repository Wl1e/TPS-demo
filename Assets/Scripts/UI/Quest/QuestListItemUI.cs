using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TPSDemo.UI
{
    /// <summary>
    /// 左侧任务列表单项 — Image 底框 + TMP 任务名，支持点击选中
    /// </summary>
    public class QuestListItemUI : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("项底框图片")]
        [SerializeField] UnityEngine.UI.Image m_Background;
        [Tooltip("任务名称文本")]
        [SerializeField] TextMeshProUGUI m_QuestName;

        public int QuestId { get; private set; }
        public Action<QuestListItemUI> OnClick;

        public void SetName(string name, int id)
        {
            m_QuestName.text = name;
            QuestId = id;
        }

        public void SetHighlight(Color color)
        {
            m_Background.color = color;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }
    }
}
