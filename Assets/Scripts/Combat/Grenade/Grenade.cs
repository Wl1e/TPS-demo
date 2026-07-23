using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    public class Grenade : NetworkBehaviour, IGrenade
    {
        [Tooltip("投掷速度")]
        [SerializeField] float m_Speed;
        public float Speed => m_Speed;
        Vector3 m_Velocity = Vector3.zero;
        int m_OwnerId;

        [Tooltip("所受重力")]
        public float Gravity = 9.81f;

        float m_AliveTime = 0f;
        [Tooltip("最大存活时间")]
        public float MaxLifeTime = 5f;
        bool m_IsRunning = false;
        bool m_IsExploded = false;

        Rigidbody m_Rigidbody;
        CapsuleCollider m_CapsuleCollider;
        AudioSource m_AudioSource;

        public Rigidbody Rigidbody => m_Rigidbody;
        public CapsuleCollider CapsuleCollider => m_CapsuleCollider;

        [Tooltip("伤害值")]
        public float Damage = 100f;
        [Tooltip("爆炸冲击力")]
        public float ExplosionForce = 1f;
        [Tooltip("爆炸范围")]
        public float ExplosionRadius = 3f;

        [Tooltip("爆炸特效预制体")]
        public GameObject ExplosionEffectPrefab;
        [Tooltip("爆炸音效")]
        public AudioClip ExplosionAudio;

        private Unity.Cinemachine.CinemachineImpulseSource m_ImpulseSource;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_CapsuleCollider = GetComponent<CapsuleCollider>();
            m_ImpulseSource = GetComponent<Unity.Cinemachine.CinemachineImpulseSource>();
            m_Rigidbody.linearDamping = 0.3f;
        }

        void Update()
        {
            if (!m_IsRunning) {
                return;
            }
            m_AliveTime += Time.deltaTime;
            if (!m_IsExploded && m_AliveTime > MaxLifeTime) {
                m_IsExploded = true;
                Explore();
                return;
            }
        }

        public void Throw(int ownerId, Vector3 dir)
        {
            m_OwnerId = ownerId;
            m_Velocity = m_Speed * dir;
            m_Rigidbody.linearVelocity = m_Velocity;
            m_IsRunning = true;
        }
        public void OnHold()
        { }
        public void OnStore()
        { }

        void Explore()
        {
            m_ImpulseSource.GenerateImpulse();
            var result = Physics.OverlapSphere(transform.position, ExplosionRadius);
            if (result.Length <= 0) {
                return;
            }
            var m_ActorManager = ActorManager.Instance;
            var attacker = ActorManager.Instance.GetActor(m_OwnerId);
            // 后续添加 IExplosionReceiver 解耦
            foreach (var collider in result) {
                if (collider.TryGetComponent(out Damageable damageable)) {
                    damageable.InflictDamage(new DamageInfo { Attacker = attacker.gameObject, Damage = Damage, Point = collider.ClosestPoint(transform.position) });
                } else if (collider.TryGetComponent(out Rigidbody rigidbody)) {
                    rigidbody.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius);
                }
            }

            Director.Instance.RequestAudio(ExplosionAudio)
                .WithMixerGroup(AudioSystem.AudioGroup.SFX)
                .WithPosition(transform.position)
                .Play();
            Director.Instance.RequestEffect(ExplosionEffectPrefab)
                .WithPosition(transform.position)
                .Create();

            NetworkObject.Despawn();
        }
    }
}
