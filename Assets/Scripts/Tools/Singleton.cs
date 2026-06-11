
using Unity.Netcode;
using UnityEngine;

public abstract class Singleton<T>: MonoBehaviour where T : Singleton<T>
{
    static T m_Instance;
    static public T Instance => m_Instance;
    protected virtual void Awake()
    {
        if(m_Instance != null) {
            Destroy(gameObject);
        }
        m_Instance = (T)this;
    }
}

public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    static T m_Instance;
    static public T Instance => m_Instance;
    protected virtual void Awake()
    {
        if (m_Instance != null) {
            Destroy(gameObject);
        }
        m_Instance = (T)this;
    }
}
