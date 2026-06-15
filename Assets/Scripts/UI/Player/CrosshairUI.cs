using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
using Event;
    public class CrosshairUI : MonoBehaviour
    {
        public Image CrosshairImage;
        public Image FeedbackImage;

        bool showCrosshair = true;

        public CrosshairData DefaultCrosshair;

        CrosshairData m_CrosshairData;
        [SerializeField] CrosshairData m_HitFeedback;

        // 更新依赖deltaTime，所以可能不准确
        public float FeedbackTime = 0.1f;
        float FeedbackEndTime = 0f;
        private void Awake()
        {
            FeedbackImage.sprite = m_HitFeedback.Sprite;
            FeedbackImage.color = m_HitFeedback.Color;
            FeedbackImage.rectTransform.sizeDelta = m_HitFeedback.Sprite.rect.size * m_HitFeedback.Scale;
            FeedbackImage.gameObject.SetActive(false);
        }
        private void OnEnable()
        {
            EventManager.AddListener<WeaponChangedEvent>(HandleWeaponChanged);
            EventManager.AddListener<BulletHitTargetEvent>(HandleHitd);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<WeaponChangedEvent>(HandleWeaponChanged);
            EventManager.RemoveListener<BulletHitTargetEvent>(HandleHitd);
        }
        public void Initialize()
        {
            m_CrosshairData = PlayerDataProxy.Instance.GetCurrentFirearm()?.Crosshair ?? DefaultCrosshair;
            UpdateCrosshair();
        }

        private void Update()
        {
            if (!showCrosshair) {
                if (FeedbackImage.IsActive() || CrosshairImage.IsActive()) {
                    CrosshairImage.gameObject.SetActive(false);
                    FeedbackImage.gameObject.SetActive(false);
                }
                return;
            }
            if (FeedbackEndTime >= Time.time) {
                FeedbackImage.gameObject.SetActive(true);
            } else {
                FeedbackImage.gameObject.SetActive(false);
            }
        }

        void HandleWeaponChanged(WeaponChangedEvent evt)
        {
            showCrosshair = true;
            m_CrosshairData = PlayerDataProxy.Instance.GetCurrentFirearm()?.Crosshair ?? DefaultCrosshair;
            UpdateCrosshair();
        }

        void UpdateCrosshair()
        {
            CrosshairImage.sprite = m_CrosshairData.Sprite;
            CrosshairImage.color = m_CrosshairData.Color;
            CrosshairImage.rectTransform.sizeDelta = m_CrosshairData.Sprite.rect.size * m_CrosshairData.Scale;
        }

        void HandleHitd(BulletHitTargetEvent evt)
        {
            if (evt.Attacker && evt.Attacker.CompareTag("Player")) {
                FeedbackEndTime = Time.time + FeedbackTime;
            }
        }
    }

}
