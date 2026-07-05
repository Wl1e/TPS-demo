using System.Collections;
using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace TPSDemo
{


    public class EnemyController : NetworkBehaviour
    {
        [Header("资源")]
        [Tooltip("受伤音效")]
        public AudioClip DamageAudio;
        [Tooltip("死亡音效")]
        public AudioClip DeadAudio;
        [Tooltip("移动音效")]
        public AudioClip MovementAudio;

        public float DiedTime = 0.3f;

        public int EnemyId = 0;

        Health m_Health;
        Actor m_Actor;
        NavMeshAgent m_Agent;
        Collider[] m_Colliders;
        HealthBar m_HealthBar;
        AudioAndEffectPlayGlobal m_AudioAndEffectPlayGlobal;

        public AudioAndEffectPlayGlobal AEPlayer => m_AudioAndEffectPlayGlobal;
        public NavMeshAgent Agent => m_Agent;

        [Tooltip("攻击者组件")]
        [SerializeField] AttackerBase m_Attacker;

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
            m_AudioAndEffectPlayGlobal = GetComponent<AudioAndEffectPlayGlobal>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) {
                if (m_Attacker != null) {
                    m_Attacker.OnAttack += OnAttack;
                }
                m_Health.OnTakeDamaged += OnTakeDamage;
                m_Health.OnDied += OnDied;
            } else {
                m_Agent.enabled = false;
                m_BehaviorTree.enabled = false;
            }
            m_HealthBar.Initialize(m_Health.Ratio);
            m_AudioAndEffectPlayGlobal.AddAudio("Dead", DeadAudio);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) {
                if (m_Attacker != null) {
                    m_Attacker.OnAttack -= OnAttack;
                }
                m_Health.OnTakeDamaged -= OnTakeDamage;
                m_Health.OnDied -= OnDied;
            }
            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (m_Agent.velocity.magnitude > 0f) {
                m_AnimatorController.Play(ManualAnimatorController.AnimationType.Walk.ToString());
            } else {
                m_AnimatorController.Play(ManualAnimatorController.AnimationType.Idle.ToString());
            }
        }

        private void OnAttack()
        {
            if (m_AnimatorController) {
                m_AnimatorController.Play(ManualAnimatorController.AnimationType.Attack.ToString());
            }
        }

        void OnTakeDamage(DamageInfo info)
        {
            if (IsServer) {
                if (m_BehaviorTree.GetVariable("m_Target", out BlackboardVariable<GameObject> target)) {
                    target.Value = info.Attacker;
                }
                m_HealthBar.UpdateHealthProgress(m_Health.Ratio);
                if (m_AnimatorController) {
                    m_AnimatorController.Play(ManualAnimatorController.AnimationType.Hit.ToString());
                }
                Director.Instance.RequestAudio(DamageAudio).WithPosition(transform.position).Play();
            }
        }

        void OnDied(int attackerId)
        {
            if (!IsServer) {
                return;
            }
            print($"{gameObject.name} IsDied");
            m_AudioAndEffectPlayGlobal.Play("Dead", float.NegativeInfinity, transform.position, Quaternion.identity);
            m_AnimatorController.Play(ManualAnimatorController.AnimationType.Died.ToString());
            EventManager.Broadcast(
                new Event.ActorDiedEvent {
                    ActorId = m_Actor.Id,
                    AttackerId = attackerId
                }
            );
            StartCoroutine(DiedCoroutine());
        }

        IEnumerator DiedCoroutine()
        {
            yield return new WaitForSeconds(DiedTime);
            if (TryGetComponent<NetworkObject>(out var no)) {
                no.Despawn();
            }
        }
    }
}
