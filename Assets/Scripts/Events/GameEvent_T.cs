using UnityEngine;
using System.Collections.Generic;

public abstract class GameEvent<T> : ScriptableObject
{
    private readonly List<System.Action<T>> m_Listeners = new List<System.Action<T>>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void RegisterListener(System.Action<T> listener)
    {
        if (!m_Listeners.Contains(listener))
        {
            m_Listeners.Add(listener);
        }
    }
    public void UnregisterListener(System.Action<T> listener)
    {
        if (m_Listeners.Contains(listener))
        {
            m_Listeners.Remove(listener);
        }
    }
    public void Raise(T t)
    {
        // listener可能取消自己，正着遍历会出问题
        for (int i = m_Listeners.Count - 1; i >= 0; i--)
        {
            m_Listeners[i].Invoke(t);
        }
    }
}
