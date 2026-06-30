using UnityEngine;

namespace TPSDemo
{
    public class GameFlowManager : Singleton<GameFlowManager>
    {
        [SerializeField] private System.Collections.Generic.List<GameObject> m_PersistentObjects;

        protected override void Awake()
        {
            base.Awake();
            foreach (var go in m_PersistentObjects) {
                if (go != null) {
                    DontDestroyOnLoad(go);
                }
            }

            // 将NetCode Prefab加载到内存
            // StartCoroutine(AssetCache.LoadByLabel("NetCodePrefab"));
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
