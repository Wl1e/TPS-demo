using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    static class NextId
    {
        static int s_NextId = 1;
        static public int GetNextId()
        {
            return s_NextId++;
        }

    }

    // Server所有的ActorId，Client只存储自己的ActorId，所以在ClientRpc中，可以用playerId来分辨Client
    public class Actor : NetworkBehaviour
    {
        private readonly NetworkVariable<int> m_Id = new(0);

        public Transform AimPoint;
        public int Id => m_Id.Value;

        private void Awake()
        {
            m_Id.OnValueChanged += OnIdChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) {
                m_Id.Value = NextId.GetNextId();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) {
                ActorManager.Instance.Actors.Remove(m_Id.Value);
            }
            base.OnNetworkDespawn();
        }

        private void OnIdChanged(int previousValue, int newValue)
        {
            ActorManager.Instance.AddActor(this);
        }

    }

    //public class Actor : MonoBehaviour
    //{
    //    private int m_Id;

    //    public Transform AimPoint;
    //    public int Id => m_Id;

    //    private void Awake()
    //    {
    //        m_Id = NextId.GetNextId();
    //        ActorManager.Instance.AddActor(this);
    //    }

    //    private void OnDestroy()
    //    {
    //        ActorManager.Instance.Actors.Remove(m_Id);
    //    }
    //}
}
