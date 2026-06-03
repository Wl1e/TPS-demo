using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace TPSDemo
{


    public class EnemyController : MonoBehaviour
    {
        public AudioClip DamageAudio;
        public AudioClip DeadAudio;

        AudioSource m_AudioSource;
        Health m_Health;
        BulletAttacker m_BulletAttacker;
        DetectModule m_DetectModule;
        Actor m_Actor;
        NavMeshAgent m_Agent;
        Collider[] m_Colliders;
        HealthBar m_HealthBar;

        [SerializeField] BehaviorGraphAgent m_BehaviorTree;

        public Health Health => m_Health;

        void Awake()
        {
            m_Actor = GetComponent<Actor>();
            m_AudioSource = GetComponent<AudioSource>();
            m_Health = GetComponent<Health>();
            m_BulletAttacker = GetComponentInChildren<BulletAttacker>();
            m_Colliders = GetComponentsInChildren<Collider>();
            m_Agent = GetComponent<NavMeshAgent>();
            m_HealthBar = GetComponentInChildren<HealthBar>();
            m_HealthBar.Initialize(m_Health.Ratio);
            gameObject.tag = "Enemy";
        }

        private void OnEnable()
        {
            m_Health.OnTakeDamaged += OnTakeDamage;
            m_Health.OnDied += OnDied;
        }

        private void OnDisable()
        {
            m_Health.OnTakeDamaged -= OnTakeDamage;
            m_Health.OnDied -= OnDied;
        }

        // Update is called once per frame
        void Update()
        {
        }

        void PlayAnimation()
        {

        }

        void OnTargetDetected()
        {
        }

        void OnTakeDamage(GameObject attacker, float damage)
        {
            m_AudioSource.PlayOneShot(DamageAudio);
            if (m_BehaviorTree.GetVariable("Target", out BlackboardVariable<GameObject> target)) {
                target.Value = attacker;
            }
            m_HealthBar.UpdateHealthProgress(m_Health.Ratio);
        }

        void OnDied()
        {
            m_AudioSource.PlayOneShot(DeadAudio);
            Destroy(gameObject);
        }
    }
}
