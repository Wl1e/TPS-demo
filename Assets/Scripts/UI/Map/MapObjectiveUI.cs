using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class MapObjectiveUI : MonoBehaviour
    {
        [SerializeField] GameObject m_EntryPrefab;
        [SerializeField] Transform m_ContentRoot;
        [SerializeField] Color m_CompletedColor = Color.green;
        [SerializeField] Color m_ActiveColor = Color.white;

        private ObjectiveDatabase m_ObjectiveDB => ResourceManager.Instance.GetResource<ObjectiveDatabase>("Objective");

        void Start()
        {
            Hide();
            EventManager.AddListener<Event.MapStateChangedEvent>(OnMapStateChanged);
            EventManager.AddListener<Event.MapObjectiveUpdateEvent>(OnObjectiveUpdate);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<Event.MapStateChangedEvent>(OnMapStateChanged);
            EventManager.RemoveListener<Event.MapObjectiveUpdateEvent>(OnObjectiveUpdate);
        }

        void OnMapStateChanged(Event.MapStateChangedEvent evt)
        {
            if (evt.State == MapState.Active) {
                var map = MapManager.Instance?.CurrentMap;
                if (map != null && map.Config.Type == MapType.Combat && map.Config.ObjConfigs.Length > 0) {
                    Show();
                } else {
                    Hide();
                }
            }
            else if (evt.State == MapState.Completed || evt.State == MapState.Idle) {
                Hide();
            }
        }

        void OnObjectiveUpdate(Event.MapObjectiveUpdateEvent evt)
        {
            if (evt.Objectives == null || evt.Objectives.Length == 0) {
                Hide();
                return;
            }

            bool allDone = true;
            foreach (var entry in evt.Objectives) {
                if (entry.Cur < entry.Max)
                    allDone = false;
            }

            if (!gameObject.activeSelf) {
                Show();
            }

            RefreshEntries(evt.Objectives);

            if (allDone) {
                Invoke(nameof(Hide), 1.5f);
            }
        }

        void Show() => gameObject.SetActive(true);

        void Hide() => gameObject.SetActive(false);

        void RefreshEntries(ObjectiveProgress[] objectives)
        {
            if (m_ContentRoot == null || m_EntryPrefab == null) {
                return;
            }

            while (m_ContentRoot.childCount < objectives.Length) {
                Instantiate(m_EntryPrefab, m_ContentRoot);
            }

            for (int i = 0; i < m_ContentRoot.childCount; i++) {
                bool active = i < objectives.Length;
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
        }
    }
}
