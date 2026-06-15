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
        Actor m_Actor;
        NavMeshAgent m_Agent;
        Collider[] m_Colliders;
        HealthBar m_HealthBar;
        
        public AttackerBase m_Attacker;

        [Tooltip("行为树")]
        [SerializeField] BehaviorGraphAgent m_BehaviorTree;

        public Health Health => m_Health;

        private ManualAnimatorController m_AnimatorController;

        private void Awake()
        {
            m_Actor = GetComponent<Actor>();
            m_Health = GetComponent<Health>();
            m_Colliders = GetComponentsInChildren<Collider>();
            m_Agent = GetComponent<NavMeshAgent>();
            m_HealthBar = GetComponentInChildren<HealthBar>();
            m_AnimatorController = GetComponentInChildren<ManualAnimatorController>();
            m_HealthBar.Initialize(m_Health.Ratio);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) {
                if (m_Attacker) {
                    m_Attacker.OnAttack += OnAttack;
                }
                m_Health.OnTakeDamaged += OnTakeDamage;
                m_Health.OnDied += OnDied;
            } else {
                m_Agent.enabled = false;
                m_BehaviorTree.enabled = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) {
                if (m_Attacker) {
                    m_Attacker.OnAttack -= OnAttack;
                }
                m_Health.OnTakeDamaged -= OnTakeDamage;
                m_Health.OnDied -= OnDied;
            }
            base.OnNetworkDespawn();
        }

        private void OnAttack()
        {
            if (m_AnimatorController) {
                m_AnimatorController.Play("Attack");
            }
        }

        void OnTakeDamage(GameObject attacker, float damage)
        {
            if (IsServer) {
                if (m_BehaviorTree.GetVariable("Target", out BlackboardVariable<GameObject> target)) {
                    target.Value = attacker;
                }
                m_HealthBar.UpdateHealthProgress(m_Health.Ratio);
                if (m_AnimatorController) {
                    m_AnimatorController.Play("Hit");
                }
                Director.Instance.RequestAudio(DamageAudio).WithPosition(transform.position).Play();
            }
        }

        void OnDied(int attackerId)
        {
            if (IsServer) {
                Director.Instance.RequestAudio(DeadAudio).WithPosition(transform.position).Play();
                EventManager.Broadcast(
                    new Event.ActorDiedEvent {
                        ActorId = m_Actor.Id,
                        AttackerId = attackerId
                    }
                );
                Destroy(gameObject);
            }
        }
    }
}
