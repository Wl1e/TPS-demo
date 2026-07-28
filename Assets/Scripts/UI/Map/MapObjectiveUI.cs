using UnityEngine;
using TMPro;

namespace TPSDemo.UI
{
    public class MapObjectiveUI : MonoBehaviour
    {
        [SerializeField] GameObject m_EntryPrefab;
        [SerializeField] Transform m_ContentRoot;
        [SerializeField] Color m_CompletedColor = Color.green;
        [SerializeField] Color m_ActiveColor = Color.white;

        private ObjectiveDatabase m_ObjectiveDB => ResourceManager.Instance.GetResource<ObjectiveDatabase>("Objective");

        private void Start()
        {
            EventManager.AddListener<Event.MapStateChangedEvent>(OnMapStateChanged);
            EventManager.AddListener<Event.MapChangeEvent>(OnEnterNewMap);
            EventManager.AddListener<Event.MapObjectiveUpdateEvent>(OnObjectiveUpdate);
            Hide();
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<Event.MapStateChangedEvent>(OnMapStateChanged);
            EventManager.RemoveListener<Event.MapChangeEvent>(OnEnterNewMap);
            EventManager.RemoveListener<Event.MapObjectiveUpdateEvent>(OnObjectiveUpdate);
        }

        /// <summary>
        /// 进入地图显示Objectives
        /// </summary>
        /// <param name="evt"></param>
        private void OnEnterNewMap(Event.MapChangeEvent evt)
        {
            RefreshEntries(PlayerDataProxy.Instance.GetCurMapObjectiveProgress());
        }

        /// <summary>
        /// 地图状态切换时刷新Objectives
        /// </summary>
        /// <param name="evt"></param>
        void OnMapStateChanged(Event.MapStateChangedEvent evt)
        {
            if (evt.State == MapState.Active) {
                var map = MapManager.Instance.CurrentMap;
                if (map != null && map.Config.Type == MapType.Combat && map.Config.ObjConfigs.Length > 0) {
                    Show();
                } else {
                    Hide();
                }
            } else if (evt.State == MapState.Completed || evt.State == MapState.Idle) {
                Hide();
            }
        }

        /// <summary>
        /// Objectives更新时
        /// </summary>
        /// <param name="evt"></param>
        void OnObjectiveUpdate(Event.MapObjectiveUpdateEvent evt)
        {
            var objectives = PlayerDataProxy.Instance.GetCurMapObjectiveProgress();
            if (objectives.Count == 0) {
                Hide();
                return;
            }

            bool allDone = true;
            foreach (var entry in objectives) {
                if (entry.Cur < entry.Max)
                    allDone = false;
            }

            Show();

            RefreshEntries(objectives);

            if (allDone) {
                Invoke(nameof(Hide), 1.5f);
            }
        }

        void Show() => gameObject.SetActive(true);

        void Hide() => gameObject.SetActive(false);

        /// <summary>
        /// 根据ObjectiveProcess更新UI
        /// </summary>
        /// <param name="objectives"></param>
        void RefreshEntries(System.Collections.Generic.List<ObjectiveProgress> objectives)
        {
            if (m_ContentRoot == null || m_EntryPrefab == null) {
                return;
            }
            print(objectives.Count);

            while (m_ContentRoot.childCount < objectives.Count) {
                Instantiate(m_EntryPrefab, m_ContentRoot);
            }

            for (int i = 0; i < m_ContentRoot.childCount; i++) {
                bool active = (i < objectives.Count);
                m_ContentRoot.GetChild(i).gameObject.SetActive(active);
                if (!active) {
                    continue;
                }

                var entry = objectives[i];
                bool completed = entry.Cur >= entry.Max;
                string desc = m_ObjectiveDB?.GetConfig(entry.ObjectiveId)?.GetObjectiveText() ?? $"m_Target {entry.ObjectiveId}";

                var texts = m_ContentRoot.GetChild(i).GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2) {
                    texts[0].text = desc;
                    texts[1].text = $"{entry.Cur} / {entry.Max}";
                    texts[0].color = completed ? m_CompletedColor : m_ActiveColor;
                    texts[1].color = completed ? m_CompletedColor : m_ActiveColor;
                }
            }

            if (objectives.Count > 0) {
                Show();
            }
        }
    }
}
