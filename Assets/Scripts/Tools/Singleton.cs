
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        static T m_Instance;
        static public T Instance => m_Instance;
        protected virtual void Awake()
        {
            if (m_Instance != null && m_Instance != this) {
                Destroy(gameObject);
            } else {
                m_Instance = (T)this;
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_Instance == this) {
                m_Instance = null;
            }
        }
    }

    public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
    {
        static T m_Instance;
        static public T Instance => m_Instance;
        protected virtual void Awake()
        {
            if (m_Instance != null && m_Instance != this) {
                Destroy(gameObject);
            } else {
                m_Instance = (T)this;
            }
        }

        public override void OnDestroy()
        {
            if (m_Instance == this) {
                m_Instance = null;
            }
        }
    }
}
