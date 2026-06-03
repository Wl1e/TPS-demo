using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class HealthUI : HealthBar
    {
        public void Initialize()
        {
            float value = PlayerDataProxy.Instance.GetHealthRatio();
            m_DelayHealth = value;
            m_CurrentHealth = value;
            SetProgress(DelayedHealth, m_DelayHealth);
            SetProgress(Bar, m_CurrentHealth);
        }

        void OnEnable()
        {
            EventManager.AddListener<Event.HealthChangedEvent>(HandleHealthChanged);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<Event.HealthChangedEvent>(HandleHealthChanged);
        }

        void HandleHealthChanged(Event.HealthChangedEvent evt)
        {
            UpdateHealthProgress(PlayerDataProxy.Instance.GetHealthRatio());
        }
    }

}
