using UnityEngine;

namespace TPSDemo
{

    public class BossArea : MonoBehaviour
    {
        [Tooltip("Boss")]
        [SerializeField] BossController m_Boss;

        [Tooltip("玩家进入boss房后设置")]
        [SerializeField] private GameObjectEventChannel m_Channel;

        [Tooltip("站立点")]
        [SerializeField] private Transform m_StandPos;

        [SerializeField] private Collider m_Area;

        private readonly System.Collections.Generic.List<PlayerController> m_Players = new();

        private void Start()
        {
            m_Boss.SetStandPos(m_StandPos.position);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.CompareTag("Player")) {
                var player = other.GetComponentInParent<PlayerController>();
                m_Players.Add(player);
                m_Channel.SendEventMessage(player.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player")) {
                var player = other.GetComponentInParent<PlayerController>();
                m_Players.Remove(player);
                if (m_Players.Count > 0) {
                    m_Channel.SendEventMessage(m_Players[0].gameObject);
                } else {
                    m_Channel.SendEventMessage(null);
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (m_StandPos != null) {
                Gizmos.color = Color.purple;
                Gizmos.DrawWireSphere(m_StandPos.position, 0.5f);
                Gizmos.DrawRay(m_StandPos.position, m_StandPos.forward * 1f);
            }
        }
#endif
    }
}
