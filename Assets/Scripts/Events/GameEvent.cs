using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Game Events/Game Event")]
public class GameEvent : ScriptableObject
{
    private readonly List<System.Action> m_Listeners = new List<System.Action>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void RegisterListener(System.Action listener)
    {
        if (!m_Listeners.Contains(listener))
        {
            m_Listeners.Add(listener);
        }
    }
    public void UnregisterListener(System.Action listener)
    {
        if (m_Listeners.Contains(listener))
        {
            m_Listeners.Remove(listener);
        }
    }
    public void Raise()
    {
        // listener可能取消自己，正着遍历会出问题
        for (int i = m_Listeners.Count - 1; i >= 0; i--)
        {
            m_Listeners[i].Invoke();
        }
    }
}
