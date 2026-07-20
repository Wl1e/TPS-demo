using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public Button StartGameBtn;
        public Button LanGameBtn;
        public Button SettingsBtn;
        public Button QuitBtn;
        public MainSettingUI MainSettingUI;

        private void Awake()
        {
            StartGameBtn.onClick.AddListener(OnStartGameClicked);
            LanGameBtn.onClick.AddListener(OnLanGameClicked);
            SettingsBtn.onClick.AddListener(OnSettingsClicked);
            QuitBtn.onClick.AddListener(OnQuitClicked);
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnStartGameClicked()
        {
            var mgr = FindAnyObjectByType<GameNetworkManager>();
            if (mgr != null) {
                mgr.HostStart();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnLanGameClicked()
        {
            var mgr = FindAnyObjectByType<GameNetworkManager>();
            if (mgr != null) {
                mgr.ClientStart();
                Cursor.lockState = CursorLockMode.Locked;
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
    }
}
