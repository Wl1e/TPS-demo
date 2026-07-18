using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class MainSettingUI : MonoBehaviour, IPanel
    {
        [System.Serializable]
        public class TabEntry
        {
            public Toggle toggle;
            public GameObject contentPanel;
        }

        public TabEntry[] tabs;

        [Header("Graphics")]
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown displayModeDropdown;
        public TMP_Dropdown qualityDropdown;

        [Header("Audio")]
        public Slider masterVolume;
        public Slider bgmVolume;
        public Slider sfxVolume;
        public AudioMixer audioMixer;
        public Vector2 volumeRange = new(-80f, 20f);

        [Header("Language")]
        public TMP_Dropdown languageDropdown;

        [Header("Navigation")]
        public Button backBtn;

        private bool m_IsOpened;
        private Resolution[] m_Resolutions;

        private void Awake()
        {
            SetupTabs();
            SetupGraphics();
            SetupAudio();
            SetupLanguage();
            if (backBtn != null) {
                backBtn.onClick.AddListener(Close);
            }

            gameObject.SetActive(false);
        }

        private void Start()
        {
            if (tabs.Length > 0 && tabs[0].toggle != null) {
                tabs[0].toggle.isOn = true;
                for (int i = 0; i < tabs.Length; i++) {
                    tabs[i].contentPanel.SetActive(i == 0);
                }
            }
        }

        private void SetupTabs()
        {
            for (int i = 0; i < tabs.Length; i++) {
                var index = i;
                tabs[i].toggle.onValueChanged.AddListener((isOn) => {
                    tabs[index].contentPanel.SetActive(isOn);
                });
            }
        }

        private void SetupGraphics()
        {
            m_Resolutions = Screen.resolutions;
            int currentResIndex = 0;
            var currentRes = Screen.currentResolution;
            for (int i = 0; i < m_Resolutions.Length; i++) {
                var r = m_Resolutions[i];
                if (r.width == currentRes.width && r.height == currentRes.height && r.refreshRateRatio.value.Equals(currentRes.refreshRateRatio.value))
                    currentResIndex = i;
            }
            List<TMP_Dropdown.OptionData> options = new();
            foreach (var res in m_Resolutions) {
                options.Add(new TMP_Dropdown.OptionData($"{res.height}x{res.width}"));
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResIndex;
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

            var mode = Screen.fullScreenMode;
            displayModeDropdown.value = mode == FullScreenMode.ExclusiveFullScreen ? 0 :
                                         mode == FullScreenMode.FullScreenWindow ? 1 : 2;
            displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);

            //if (qualityDropdown != null) {
            //    qualityDropdown.ClearOptions();
            //    qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
            //    qualityDropdown.value = QualitySettings.GetQualityLevel();
            //    qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            //}
        }

        private void SetupAudio()
        {
            if (audioMixer != null) {
                if (audioMixer.GetFloat("Master", out var v)) {
                    masterVolume.value = Mathf.Clamp01((v - volumeRange.x) / (volumeRange.y - volumeRange.x));
                }
                if (audioMixer.GetFloat("BGM", out v)) {
                    bgmVolume.value = Mathf.Clamp01((v - volumeRange.x) / (volumeRange.y - volumeRange.x));
                }
                if (audioMixer.GetFloat("SFX", out v)) {
                    sfxVolume.value = Mathf.Clamp01((v - volumeRange.x) / (volumeRange.y - volumeRange.x));
                }
            }
            masterVolume.onValueChanged.AddListener(v => SetVolume("Master", v));
            bgmVolume.onValueChanged.AddListener(v => SetVolume("BGM", v));
            sfxVolume.onValueChanged.AddListener(v => SetVolume("SFX", v));
        }

        private void SetupLanguage()
        {
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        private void OnResolutionChanged(int index)
        {
            if (index < 0 || index >= m_Resolutions.Length) {
                return;
            }
            var r = m_Resolutions[index];
            Screen.SetResolution(r.width, r.height, Screen.fullScreenMode, r.refreshRateRatio);
        }

        private void OnDisplayModeChanged(int index)
        {
            var mode = index switch
            {
                0 => FullScreenMode.FullScreenWindow,
                1 => FullScreenMode.MaximizedWindow,
                _ => FullScreenMode.Windowed,
            };
            Screen.fullScreenMode = mode;
        }

        //private void OnQualityChanged(int index)
        //{
        //    QualitySettings.SetQualityLevel(index, true);
        //}

        private void SetVolume(string name, float value)
        {
            if (audioMixer == null) {
                return;
            }
            audioMixer.SetFloat(name, Mathf.Lerp(volumeRange.x, volumeRange.y, value));
        }

        private void OnLanguageChanged(int opt)
        {
            EventManager.Broadcast(new Event.SettingChangedEvent { Language = opt });
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
    }
}
