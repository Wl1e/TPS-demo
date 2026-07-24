using System;
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
        public AudioClip HitAudio;
        [Tooltip("死亡音效")]
        public AudioClip DeadAudio;
        [Tooltip("移动音效")]
        public AudioClip MovementAudio;

        [Tooltip("被攻击时设置Target")]
        [SerializeField] private bool m_SetTargetWhenHit = true;
        // Fixme: 相同enemy共用了管道
        [Tooltip("管道")]
        [SerializeField] protected GameObjectEventChannel m_Channel;

        [Tooltip("旋转时间")]
        [SerializeField] private float m_SmoothRotateTime = 1f;
        //private float m_SmoothVelocity = 0f;
        private Vector3 m_TargetDir = Vector3.zero;

        public float DiedTime = 0.3f;

        public int EnemyId = 0;

        // 目前BossController继承使用EnemyController逻辑，所以暂时改成Protected
        protected Health m_Health;
        protected Actor m_Actor;
        protected UnityEngine.AI.NavMeshAgent m_Agent;
        protected HealthBar m_HealthBar;
        protected AudioAndEffectPlayGlobal m_AudioAndEffectPlayGlobal;

        protected SkillController m_SkillController = null;

        public SkillController SkillController => m_SkillController;

        public UnityEngine.AI.NavMeshAgent Agent => m_Agent;
        public AudioAndEffectPlayGlobal AEPlayer => m_AudioAndEffectPlayGlobal;

        public bool IsDied => m_Health.IsDied;

        [Tooltip("攻击者组件")]
        [SerializeField] protected AttackerBase m_Attacker;

        [Tooltip("行为树")]
        [SerializeField] protected BehaviorGraphAgent m_BehaviorTree;

        public Health Health => m_Health;

        private PlayableController m_AnimatorController;
        public PlayableController AnimatorController => m_AnimatorController;
        public Hitbox EnemyHitbox;

        private void Awake()
        {
            m_Actor = GetComponent<Actor>();
            m_Health = GetComponent<Health>();
            m_Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            m_HealthBar = GetComponentInChildren<HealthBar>();
            m_AnimatorController = GetComponentInChildren<PlayableController>();
            m_AudioAndEffectPlayGlobal = GetComponent<AudioAndEffectPlayGlobal>();
            TryGetComponent(out m_SkillController);
            m_TargetDir = transform.forward;
            m_TargetDir.y = 0;
        }

        private void Update()
        {
            //if(transform.forward != m_TargetDir) {
            //    var forward = transform.forward;
            //    var curAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            //    var targetAngle = Mathf.Atan2(m_TargetDir.x, m_TargetDir.z) * Mathf.Rad2Deg;
            //    var d = Mathf.SmoothDampAngle(curAngle, targetAngle, ref m_SmoothVelocity, m_SmoothRotateTime);
            //    transform.rotation = Quaternion.Euler(0, d, 0);
            //}
            if(m_AnimatorController.CurrentClipName == "Walk" && m_Agent.velocity == Vector3.zero) {
                m_AnimatorController.Stop();
            } else if(m_AnimatorController.CurrentClipName == "Idle" && m_Agent.velocity != Vector3.zero) {
                m_AnimatorController.Play("Walk");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner) {
                if (m_Attacker != null) {
                    m_Attacker.OnAttack += OnAttack;
                }
                m_Health.OnTakeDamaged += OnTakeDamage;
                m_Health.OnDied += OnDied;
            } else {
                m_Agent.enabled = false;
                m_BehaviorTree.enabled = false;
            }
            m_Health.OnHealthChanged += OnHealthChanged;
            m_HealthBar.Initialize(m_Health.Ratio);
            //NetworkEffectService.Instance.AddAudio(DeadAudio);
            m_AudioAndEffectPlayGlobal.AddAudio("Died", DeadAudio);
            m_AudioAndEffectPlayGlobal.AddAudio("Hit", HitAudio);
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner) {
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

        private void OnTakeDamage(DamageInfo info)
        {
            if (m_AnimatorController) {
                m_AnimatorController.Play("Hit");
            }
            AEPlayer.Play("Hit", AudioSystem.AudioGroup.SFX,
                float.PositiveInfinity, info.Point, Quaternion.identity);

            print($"{m_SetTargetWhenHit}, {m_Channel}");
            if(m_SetTargetWhenHit && m_Channel) {
                m_Channel.SendEventMessage(info.Attacker);
            }
        }

        private void OnHealthChanged(float obj)
        {
            m_HealthBar.UpdateHealthProgress(m_Health.Ratio);
        }

        void OnDied(int attackerId)
        {
            if (!IsOwner) {
                return;
            }
            print($"{gameObject.name} IsDied");
            //NetworkEffectService.Instance.Play(
            //    DeadAudio.name, float.NegativeInfinity, transform.position, Quaternion.identity
            //);
            m_AudioAndEffectPlayGlobal.Play(
                "Died", AudioSystem.AudioGroup.SFX, float.NegativeInfinity, transform.position, Quaternion.identity
            );
            m_AnimatorController.Play("Died");
            EventManager.Broadcast(
                new Event.ActorDiedEvent {
                    ActorId = m_Actor.Id,
                    AttackerId = attackerId
                }
            );
            StartCoroutine(DiedCoroutine());
        }

        /// <summary>
        /// 死亡协程
        /// </summary>
        /// <returns></returns>
        IEnumerator DiedCoroutine()
        {
            yield return new WaitForSeconds(DiedTime);
            if (NetworkObject.IsSpawned) {
                NetworkObject.Despawn();
            }
        }

        /// <summary>
        /// 平滑地看向某个方向（不可用）
        /// </summary>
        /// <param name="dir"></param>
        public void LookTo(Vector3 dir) => m_TargetDir = dir;

        public void SetTarget(GameObject player) => m_Channel.SendEventMessage(player);
    }
}
