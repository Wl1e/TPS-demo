using UnityEngine;

namespace TPSDemo
{
    static public class Language
    {
        public enum LanguageEnum: int
        {
            Chinese = 0,
            English = 1,
        }

        public const string Chinese = "zh-CN";
        public const string English = "en-US";

        static public string GetLanguageString(LanguageEnum language) => language switch {
            LanguageEnum.Chinese => Chinese,
            LanguageEnum.English => English,
            _ => null
        };
    }
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        public Language.LanguageEnum DefaultLanguage = Language.LanguageEnum.Chinese;
        private Language.LanguageEnum m_CurLanguage;
        private bool m_Updating = false;

        public Language.LanguageEnum CurLanguage => m_CurLanguage;

        private void Start()
        {
            m_CurLanguage = DefaultLanguage;
            SwitchLanguage(m_CurLanguage);
            EventManager.AddListener<Event.SettingChangedEvent>(OnLanguageChanged);
        }

        private void OnLanguageChanged(Event.SettingChangedEvent evt)
        {
            if((Language.LanguageEnum)evt.Language == m_CurLanguage) {
                return;
            }
            SwitchLanguage((Language.LanguageEnum)evt.Language);
        }

        private void SwitchLanguage(Language.LanguageEnum language)
        {
            print(" ");
            if(m_Updating) {
                print("Updating");
                return;
            }
            m_Updating = true;
            // 移除旧语言相关资产
            AssetCache.ReleaseByLabel(Language.GetLanguageString(m_CurLanguage));
            // 载入新语言资产
            StartCoroutine(AssetCache.DownloadDependencies(Language.GetLanguageString(language), true,
                () => {
                    m_CurLanguage = language;
                    EventManager.Broadcast(new Event.LanguageChangedEvent { NewLanguage = m_CurLanguage });
                    m_Updating = false;
                }
            ));
        }

        public string GetCurrentLanguageString() => Language.GetLanguageString(m_CurLanguage);

    }
}
