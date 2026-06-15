using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Tooltip("血条背景")]
    public Image Background;
    [Tooltip("延迟血条")]
    public Image DelayedHealth;
    [Tooltip("血条")]
    public Image Bar;

    [Tooltip("延迟事件")]
    public float DelayTime = 0.2f;
    private float DelaySpeed;
    // 延迟血量
    protected float m_DelayHealth;
    // 当前血量
    protected float m_CurrentHealth;

    public void Initialize(float value)
    {
        m_DelayHealth = value;
        m_CurrentHealth = value;
        SetProgress(DelayedHealth, m_DelayHealth);
        SetProgress(Bar, m_CurrentHealth);
    }

    protected void SetProgress(Image bar, float health)
    {
        var rect = bar.rectTransform;
        rect.anchorMax = new Vector2(health, rect.anchorMax.y);
        rect.offsetMax = new Vector2(0, 0);
    }

    void Update()
    {
        if (m_DelayHealth != m_CurrentHealth) {
            var value = Mathf.SmoothDamp(m_DelayHealth, m_CurrentHealth, ref DelaySpeed, DelayTime);
            SetProgress(DelayedHealth, value);
            m_DelayHealth = value;
        }
    }

    public void UpdateHealthProgress(float value)
    {
        m_CurrentHealth = value;
        SetProgress(Bar, m_CurrentHealth);
    }
}
