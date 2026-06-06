using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace TPSDemo
{


    public class EnemyController : NetworkBehaviour
    {
        // resource
        [Tooltip("受伤音效")]
        public AudioClip DamageAudio;
        [Tooltip("死亡音效")]
        public AudioClip DeadAudio;

        Health m_Health;
        BulletAttacker m_BulletAttacker;
        Actor m_Actor;
        NavMeshAgent m_Agent;
        Collider[] m_Colliders;
        HealthBar m_HealthBar;
        

        [Tooltip("行为树")]
        [SerializeField] BehaviorGraphAgent m_BehaviorTree;

        public Health Health => m_Health;

        private void Awake()
        {
            m_Actor = GetComponent<Actor>();
            m_Health = GetComponent<Health>();
            m_BulletAttacker = GetComponentInChildren<BulletAttacker>();
            m_Colliders = GetComponentsInChildren<Collider>();
            m_Agent = GetComponent<NavMeshAgent>();
            m_HealthBar = GetComponentInChildren<HealthBar>();
            m_HealthBar.Initialize(m_Health.Ratio);
            gameObject.tag = "Enemy";
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) {
                m_Agent.enabled = false;
                m_BulletAttacker.enabled = false;
                m_BehaviorTree.enabled = false;
            } else {
                m_Health.OnTakeDamaged += OnTakeDamage;
                m_Health.OnDied += OnDied;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!IsClient) {
                m_Health.OnTakeDamaged -= OnTakeDamage;
                m_Health.OnDied -= OnDied;
            }
        }

        void OnTakeDamage(GameObject attacker, float damage)
        {
            if (IsServer) {
                Director.Instance.RequestAudio(DamageAudio).AttachTo(transform).Play();
                if (m_BehaviorTree.GetVariable("Target", out BlackboardVariable<GameObject> target)) {
                    target.Value = attacker;
                }
                m_HealthBar.UpdateHealthProgress(m_Health.Ratio);
            }
        }

        void OnDied()
        {
            if (IsServer) {
                Director.Instance.RequestAudio(DeadAudio).WithPosition(transform.position).Play();
                Destroy(gameObject);
            }
        }
    }
}
