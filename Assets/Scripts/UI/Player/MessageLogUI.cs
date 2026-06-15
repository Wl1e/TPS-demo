using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo.UI
{
    /// <summary>
    /// 消息日志主控 — 监听 MessageLogEvent，在 HUD 上方显示消息，自动淡出
    /// </summary>
    public class MessageLogUI : MonoBehaviour
    {
        [Tooltip("消息单项预制体")]
        [SerializeField] MessageEntryUI m_MessagePrefab;
        [Tooltip("消息容器")]
        [SerializeField] Transform m_Container;
        [SerializeField] float m_DefaultTime = 3f;

        // 需要定期清除吗
        readonly Queue<MessageEntryUI> m_Pool = new();
        readonly List<MessageEntryUI> m_Active = new();

        void OnEnable()
        {
            EventManager.AddListener<Event.MessageLogEvent>(OnMessage);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<Event.MessageLogEvent>(OnMessage);
        }

        void OnMessage(Event.MessageLogEvent evt)
        {
            var entry = GetEntry();
            entry.Show(evt.Message, evt.Duration == -1f ? m_DefaultTime : evt.Duration);
            m_Active.Add(entry);

            // 清除已完成的消息引用
            foreach(var e in m_Active) {
                if(!e.gameObject.activeSelf) {
                    m_Pool.Enqueue(e);
                }
            }
        }

        MessageEntryUI GetEntry()
        {
            if (m_Pool.TryDequeue(out var entry) && entry != null) {
                entry.transform.SetAsLastSibling();
                return entry;
            }

            entry = Instantiate(m_MessagePrefab, m_Container);
            return entry;
        }
    }
}
