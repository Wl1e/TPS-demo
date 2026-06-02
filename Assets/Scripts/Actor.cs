using System;
using Unity.VisualScripting;
using UnityEngine;

static class NextId
{
    static int s_NextId = 0;
    static public int GetNextId()
    {
        return s_NextId++;
    }

}

public class Actor : MonoBehaviour
{
    int m_Id;
    public int Affiliation;
    ActorManager m_ActorManager = null;

    public Transform AimPoint;
    public int Id => m_Id;

    private void Awake()
    {
        m_Id = NextId.GetNextId();
    }

    void Start()
    {
        m_ActorManager = FindAnyObjectByType<ActorManager>();
        if (m_ActorManager && !m_ActorManager.Actors.ContainsKey(m_Id)) {
            m_ActorManager.AddActor(this);
        }
    }

    private void OnDestroy()
    {
        if (m_ActorManager) {
            m_ActorManager.Actors.Remove(m_Id);
        }
    }

    public bool IsHostile(Actor actor)
    {
        return Affiliation != actor.Affiliation;
    }
}
