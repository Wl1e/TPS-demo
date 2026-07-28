using UnityEngine;

namespace TPSDemo
{
	public class MapPortal: MonoBehaviour
	{
        [SerializeField] private Collider Portal;
        [SerializeField] private int TargetMapId = -1;
        [SerializeField] private GameObject m_Effect;

        private bool m_IsActive = true;

        private void Start()
        {
            var map = MapManager.Instance.CurrentMap;
            if(!map) {
                map = FindAnyObjectByType<Map>();
            }
            SetPortalState(map.State == MapState.Completed);
            EventManager.AddListener<Event.MapStateChangedEvent>(OnMapStateChanged);
        }

        private void OnMapStateChanged(Event.MapStateChangedEvent evt)
        {
            SetPortalState(evt.State == MapState.Completed);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<Event.MapStateChangedEvent>(OnMapStateChanged);
        }

        public void SetPortalState(bool isActive)
        {
            m_IsActive = isActive;

            Portal.enabled = m_IsActive;
            m_Effect.SetActive(m_IsActive);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (TargetMapId != -1 && other.gameObject.CompareTag("Player")) {
                MapManager.Instance.EnterMap(TargetMapId);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            
        }
    }
}
