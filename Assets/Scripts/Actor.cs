using Unity.Netcode;
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

    public class Actor : NetworkBehaviour
    {
        private NetworkVariable<int> m_Id = new(0);

        public Transform AimPoint;
        public int Id => m_Id.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) {
                m_Id.Value = NextId.GetNextId();
                ActorManager.Instance.AddActor(this);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) {
                ActorManager.Instance.Actors.Remove(m_Id.Value);
            }
            base.OnNetworkDespawn();
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
