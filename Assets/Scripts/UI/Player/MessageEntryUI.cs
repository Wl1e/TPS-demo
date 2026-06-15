using System.Collections;
using TMPro;
using UnityEngine;

namespace TPSDemo.UI
{
    /// <summary>
    /// 消息日志单项 — Image 底框 + TMP 文本，自动淡出销毁
    /// </summary>
    public class MessageEntryUI : MonoBehaviour
    {
        [Tooltip("背景图片")]
        [SerializeField] UnityEngine.UI.Image m_Background;
        [Tooltip("消息文本")]
        [SerializeField] TextMeshProUGUI m_Text;
        [Tooltip("淡出时长(秒)")]
        [SerializeField] float m_FadeOutTime = 0.5f;

        private bool m_StartFade = false;
        private float m_RemainTime = 0f;

        private void Update()
        {
            m_RemainTime -= Time.deltaTime;
            if (!m_StartFade && m_RemainTime <= m_FadeOutTime) {
                m_StartFade = true;
                m_Background.CrossFadeAlpha(0f, m_FadeOutTime, true);
            } else if(m_RemainTime <= 0) {
                gameObject.SetActive(false);
            }
        }

        public void Show(string message, float duration = -1f)
        {
            if (duration < 0f) {
                return;
            }
            m_StartFade = false;
            gameObject.SetActive(true);

            m_RemainTime = duration + m_FadeOutTime;
            m_Text.text = message;
            
        }
    }
}
