using UnityEngine;

namespace TPSDemo
{
    public class GameFlowManager : Singleton<GameFlowManager>
    {
        [SerializeField] System.Collections.Generic.List<GameObject> m_PersistentObjects;
        protected override void Awake()
        {
            base.Awake();
            foreach (var go in m_PersistentObjects) {
                DontDestroyOnLoad(go);
            }
        }

        public void SetDDOL(GameObject go)
        {
            if (m_PersistentObjects.Contains(go)) {
                return;
            }
            m_PersistentObjects.Add(go);
            DontDestroyOnLoad(go);
        }
    }
}
