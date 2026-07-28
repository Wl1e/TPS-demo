using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class VoidDeathChecker: NetworkBehaviour
    {
        [Tooltip("致死高度")]
        [SerializeField] private float m_DeathY = -5f;
        [Tooltip("伤害")]
        [SerializeField] private float m_Damage = float.MaxValue;
        [Tooltip("检查间隔")]
        [SerializeField] private int m_CheckInterval = 3;
        private int m_Time = 0;

        private ActorManager m_ActorManager;

        private void Start()
        {
            m_ActorManager = ActorManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            if(!IsServer) {
                return;
            }
            m_Time++;
            if(m_Time < m_CheckInterval) {
                return;
            }
            m_Time = 0;

            foreach (var actor in m_ActorManager.Actors.Values) {
                if (actor.transform.position.y > m_DeathY) {
                    continue;
                }

                if (!actor.TryGetComponent<Health>(out var health)) {
                    Debug.LogError($"Actor{actor.Id} dont have health");
                }

                health.TakeDamage(new DamageInfo {
                    Attacker = null, Damage = m_Damage, Point = actor.AimPoint.position
                });
            }
        }
    }
}
