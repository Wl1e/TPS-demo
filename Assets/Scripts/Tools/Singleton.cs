
using UnityEngine;

public abstract class Singleton<T>: MonoBehaviour where T : Singleton<T>
{
    static T m_Instance;
    static public T Instance => m_Instance;
    private void Awake()
    {
        if(m_Instance != null) {
            Destroy(gameObject);
        }
        m_Instance = (T)this;
    }
}
