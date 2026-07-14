using System.Collections;
using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;

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

        [Tooltip("被攻击时设置Target")]
        [SerializeField] private bool m_SetTargetWhenHit = true;
        [Tooltip("管道")]
        [SerializeField] protected GameObjectEventChannel m_Channel;

        [Tooltip("旋转时间")]
        [SerializeField] private float m_SmoothRotateTime = 1f;
        private float m_SmoothVelocity = 0f;
        private Vector3 m_TargetDir = Vector3.zero;

        public float DiedTime = 0.3f;

        public int EnemyId = 0;

        // 目前BossController继承使用EnemyController逻辑，所以暂时改成Protected
        protected Health m_Health;
        protected Actor m_Actor;
        protected UnityEngine.AI.NavMeshAgent m_Agent;
        protected HealthBar m_HealthBar;
        protected AudioAndEffectPlayGlobal m_AudioAndEffectPlayGlobal;

        public UnityEngine.AI.NavMeshAgent Agent => m_Agent;
        public AudioAndEffectPlayGlobal AEPlayer => m_AudioAndEffectPlayGlobal;

        [Tooltip("攻击者组件")]
        [SerializeField] protected AttackerBase m_Attacker;

        [Tooltip("行为树")]
        [SerializeField] protected BehaviorGraphAgent m_BehaviorTree;

        public Health Health => m_Health;

        private ManualAnimatorController m_AnimatorController;
        public ManualAnimatorController AnimatorController => m_AnimatorController;
        public Hitbox EnemyHitbox;

        private void Awake()
        {
            m_Actor = GetComponent<Actor>();
            m_Health = GetComponent<Health>();
            m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            m_HealthBar = GetComponentInChildren<HealthBar>();
            m_AnimatorController = GetComponentInChildren<ManualAnimatorController>();
            m_TargetDir = transform.forward;
            m_TargetDir.y = 0;
        }

        private void Update()
        {
            if(transform.forward != m_TargetDir) {
                var forward = transform.forward;
                var curAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
                var targetAngle = Mathf.Atan2(m_TargetDir.x, m_TargetDir.z) * Mathf.Rad2Deg;
                var d = Mathf.SmoothDampAngle(curAngle, targetAngle, ref m_SmoothVelocity, m_SmoothRotateTime);
                transform.rotation = Quaternion.Euler(0, d, 0);
            }
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
            //NetworkEffectService.Instance.AddAudio(DeadAudio);
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

        private void OnAttack()
        {
        }

        void OnTakeDamage(DamageInfo info)
        {
            if (IsServer) {
                m_HealthBar.UpdateHealthProgress(m_Health.Ratio);
                if (m_AnimatorController) {
                    m_AnimatorController.Play(ManualAnimatorController.AnimationType.Hit.ToString());
                }
                Director.Instance.RequestAudio(DamageAudio).WithPosition(transform.position).Play();

                if(m_SetTargetWhenHit && m_Channel) {
                    m_Channel.SendEventMessage(info.Attacker);
                }
            }
        }

        void OnDied(int attackerId)
        {
            if (!IsServer) {
                return;
            }
            print($"{gameObject.name} IsDied");
            //NetworkEffectService.Instance.Play(
            //    DeadAudio.name, float.NegativeInfinity, transform.position, Quaternion.identity
            //);
            m_AudioAndEffectPlayGlobal.Play(
                "Dead", float.NegativeInfinity, transform.position, Quaternion.identity
            );
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
            NetworkObject.Despawn();
        }

        public void LookTo(Vector3 dir) => m_TargetDir = dir;
    }
}
