using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class SettingUI : MonoBehaviour, IPanel
    {
        public TMP_Dropdown Language;
        public Slider MasterVolume;
        public Slider BGMVolume;
        public Slider SFXVolume;
        public Button ReturnToMenuBtn;

        public Vector2 VolumeRange = new(-80f, 20f);

        [SerializeField] private GameEvent m_OpenSetting;
        private bool m_IsOpened = false;

        [SerializeField] private UnityEngine.Audio.AudioMixer m_Mixer;

        private void Awake()
        {
            gameObject.SetActive(m_IsOpened);
        }

        public void Initialize()
        {
            Language.onValueChanged.AddListener(OnLanguageChanged);
            MasterVolume.onValueChanged.AddListener(UpdateMasterVolume);
            BGMVolume.onValueChanged.AddListener(UpdateBGMVolume);
            SFXVolume.onValueChanged.AddListener(UpdateSFXVolume);
            ReturnToMenuBtn.onClick.AddListener(OnReturnToMenuClicked);
            m_Mixer.GetFloat("Master", out var volume);
            MasterVolume.value = Mathf.Clamp01(
                (volume - VolumeRange.x) / (VolumeRange.y - VolumeRange.x)
            );
            m_Mixer.GetFloat("BGM", out volume);
            BGMVolume.value = Mathf.Clamp01(
                (volume - VolumeRange.x) / (VolumeRange.y - VolumeRange.x)
            );
            m_Mixer.GetFloat("SFX", out volume);
            SFXVolume.value = Mathf.Clamp01(
                (volume - VolumeRange.x) / (VolumeRange.y - VolumeRange.x)
            );
        }

        private void OnLanguageChanged(int opt)
        {
            EventManager.Broadcast(new Event.SettingChangedEvent { Language = opt });
        }

        private void UpdateMasterVolume(float volume) => UpdateVolume(0, volume);
        private void UpdateBGMVolume(float volume) => UpdateVolume(1, volume);
        private void UpdateSFXVolume(float volume) => UpdateVolume(2, volume);

        private void UpdateVolume(int idx, float volume)
        {
            if (idx == 0) {
                m_Mixer.SetFloat("Master", Mathf.Lerp(VolumeRange.x, VolumeRange.y, volume));
            } else if (idx == 1) {
                m_Mixer.SetFloat("BGM", Mathf.Lerp(VolumeRange.x, VolumeRange.y, volume));
            } else if (idx == 2) {
                m_Mixer.SetFloat("SFX", Mathf.Lerp(VolumeRange.x, VolumeRange.y, volume));
            }
        }

        public void Open()
        {
            m_IsOpened = true;
            gameObject.SetActive(m_IsOpened);
            if (ReturnToMenuBtn != null) {
                ReturnToMenuBtn.gameObject.SetActive(true);
            }
        }

        public void Close()
        {
            m_IsOpened = false;
            gameObject.SetActive(m_IsOpened);
            if (ReturnToMenuBtn != null) {
                ReturnToMenuBtn.gameObject.SetActive(false);
            }
        }

        private void OnReturnToMenuClicked()
        {
            m_IsOpened = false;
            gameObject.SetActive(m_IsOpened);
            var mgr = GameNetworkManager.Instance;
            if (mgr != null && mgr.IsListening) {
                mgr.Disconnect();
            }
            SceneManager.LoadScene("Boot");
            UIController.Instance.CloseAllUI();
        }
    }
}
