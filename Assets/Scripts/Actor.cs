using System;
using Unity.VisualScripting;
using UnityEngine;

namespace TPSDemo
{

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
        private int m_Id;
        private ActorManager m_ActorManager = null;

        public Transform AimPoint;
        public int Id => m_Id;

        private void Awake()
        {
            m_Id = NextId.GetNextId();
        }

        void Start()
        {
            m_ActorManager = ActorManager.Instance;
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
    }
}
