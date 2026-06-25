using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
using Event;
    public class CrosshairUI : MonoBehaviour
    {
        public Image CrosshairImage;
        public Image FeedbackImage;

        private bool m_ShowCrosshair = true;
        public bool ShowCrosshair
        {
            get => m_ShowCrosshair;
            set
            {
                m_ShowCrosshair = value;
                CrosshairImage.gameObject.SetActive(m_ShowCrosshair);
                FeedbackImage.gameObject.SetActive(m_ShowCrosshair);
            }
        }

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
        public void Initialize() => UpdateCrosshair();

        private void Update()
        {
            if (FeedbackEndTime >= Time.time) {
                FeedbackImage.gameObject.SetActive(true);
            } else {
                FeedbackImage.gameObject.SetActive(false);
            }
        }

        void HandleWeaponChanged(WeaponChangedEvent evt)
        {
            UpdateCrosshair();
        }

        void UpdateCrosshair()
        {
            if(!m_ShowCrosshair) {
                return;
            }
            var crosshair = PlayerDataProxy.Instance.GetCurrentFirearm()?.Crosshair;
            if (crosshair.HasValue && crosshair.Value.Sprite != null) {
                m_CrosshairData = crosshair.Value;
            } else {
                m_CrosshairData = DefaultCrosshair;
            }
            print("m_CrosshairData: " + m_CrosshairData.Sprite);
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
