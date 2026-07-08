using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    /// <summary>
    /// 需要放在Enemy的下一级
    /// </summary>
	public class MeleeAttacker: AttackerBase
    {
        [Tooltip("攻击盒持续时间（需小于等于攻击间隔）")]
        [SerializeField] private float m_Time = 0.1f;
        [Tooltip("攻击盒")]
        [SerializeField] private Hitbox m_Hitbox;

        [Header("资源")]
        [Tooltip("攻击音效")]
        [SerializeField] private AudioClip m_AttackSfx;
        [SerializeField] AnimationClip m_AttackAnimation;

        private Coroutine m_Coroutine;

        private const string m_SFName = "MeleeAttack";

        public override void OnNetworkSpawn()
        {
            if (IsOwner) {
                m_Hitbox.OnCollision += OnCollisionPlayerHurtbox;
                Owner.AEPlayer.AddAudio(m_SFName, m_AttackSfx);
            }
        }

        public override void Attack(Transform target)
        {
            if (!IsOwner) {
                return;
            }

            if(!CanAttack()) {
                return;
            }

            Vector3 pos = target.position;
            if(target.TryGetComponent<Actor>(out var actor)) {
                pos = actor.AimPoint.position;
            }

            transform.LookAt(pos);
            m_Hitbox.SetEnable(true);

            if (m_Coroutine != null) {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }

            WhenAttack();
            PlayerAE();
            m_Coroutine = StartCoroutine(CloseHitboxCoroutine());
        }

        protected void PlayerAE()
        {
            Owner.AEPlayer.Play(m_SFName, float.PositiveInfinity, transform.position, transform.rotation);
            Owner.AnimatorController.RegisterAnimationClip("Attack", m_AttackAnimation);
            Owner.AnimatorController.Play("Attack");
        }

        private IEnumerator CloseHitboxCoroutine()
        {
            yield return new WaitForSeconds(m_Time);
            m_Hitbox.SetEnable(false);
        }

        private void OnCollisionPlayerHurtbox(Damageable damageable)
        {
            damageable.InflictDamage(new DamageInfo { Attacker = transform.parent.gameObject, Damage = m_Damage, Point = damageable.transform.position });
        }
    }
}
