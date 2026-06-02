using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    // 血条背景
    public Image Background;
    // 延迟血条
    public Image DelayedHealth;
    // 血条
    public Image Bar;

    // 延迟血条的插值速度
    public float Speed;
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
            var value = Mathf.Lerp(m_DelayHealth, m_CurrentHealth, Speed);
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
