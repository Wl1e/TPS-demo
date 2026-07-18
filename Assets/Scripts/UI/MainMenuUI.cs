using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class MainMenuUI : MonoBehaviour, IPanel
    {
        public Button StartGameBtn;
        public Button LanGameBtn;
        public Button SettingsBtn;
        public Button QuitBtn;
        public MainSettingUI MainSettingUI;

        private bool m_IsOpened = true;

        private void Awake()
        {
            StartGameBtn.onClick.AddListener(OnStartGameClicked);
            LanGameBtn.onClick.AddListener(OnLanGameClicked);
            SettingsBtn.onClick.AddListener(OnSettingsClicked);
            QuitBtn.onClick.AddListener(OnQuitClicked);
            

            gameObject.SetActive(m_IsOpened);
        }

        public void Open()
        {
            m_IsOpened = true;
            gameObject.SetActive(true);
        }

        public void Close()
        {
            m_IsOpened = false;
            gameObject.SetActive(false);
        }

        private void OnStartGameClicked()
        {
            Close();
            var mgr = FindAnyObjectByType<GameNetworkManager>();
            if (mgr != null) {
                mgr.HostStart();
            }
        }

        private void OnLanGameClicked()
        {
            Close();
            var mgr = FindAnyObjectByType<GameNetworkManager>();
            if (mgr != null) {
                mgr.ClientStart();
            }
        }

private void OnSettingsClicked()
        {
            if (MainSettingUI == null) {
                return;
            }
            MainSettingUI.Open();
        }

        private static void OnQuitClicked()
        {
            Application.Quit();
        }

        private static void OnClientStarted()
        {
        }

        private static void OnClientStopped(bool isHost)
        {
        }
    }
}
