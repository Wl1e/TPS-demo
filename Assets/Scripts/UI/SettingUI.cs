using TMPro;
using UnityEngine;

namespace TPSDemo.UI
{
    public class SettingUI : MonoBehaviour, IPanel
    {
        public TMP_Dropdown Language;

        [SerializeField] private GameEvent m_OpenSetting;
        private bool m_IsOpened = false;

        private void Start()
        {
            Language.onValueChanged.AddListener(OnLanguageChanged);
            gameObject.SetActive(m_IsOpened);
        }

        public void Initialize()
        { }

        private void OnLanguageChanged(int opt)
        {
            EventManager.Broadcast(new Event.SettingChangedEvent { Language = opt });
        }

        public void Open()
        {
            m_IsOpened = true;
            gameObject.SetActive(m_IsOpened);
        }

        public void Close()
        {
            m_IsOpened = false;
            gameObject.SetActive(m_IsOpened);
        }
    }
}
