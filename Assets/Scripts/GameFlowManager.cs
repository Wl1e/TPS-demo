using UnityEngine;

namespace TPSDemo
{
    public class GameFlowManager : MonoBehaviour
    {
        [SerializeField] System.Collections.Generic.List<GameObject> m_PersistentObjects;
        private void Awake()
        {
            foreach (var go in m_PersistentObjects) {
                DontDestroyOnLoad(go);
            }
        }
    }
}
