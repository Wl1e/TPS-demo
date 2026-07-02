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

        private Coroutine m_Coroutine;

        protected void Awake()
        {
            m_Hitbox.OnCollision += OnCollisionPlayerHurtbox;
        }

        public override void Attack(Vector3 pos)
        {
            if (!IsServer) {
                return;
            }

            transform.LookAt(pos);
            m_Hitbox.SetEnable(true);

            if (m_Coroutine != null) {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }

            WhenAttack();
            HandleShootClientRpc();
            m_Coroutine = StartCoroutine(CloseHitboxCoroutine());
        }

        private IEnumerator CloseHitboxCoroutine()
        {
            yield return new WaitForSeconds(m_Time);
            m_Hitbox.SetEnable(false);
        }

        private void OnCollisionPlayerHurtbox(Damageable damageable)
        {
            damageable.InflictDamage(transform.parent.gameObject, m_Damage);
        }

        [ClientRpc]
        public void HandleShootClientRpc()
        {
            Director.Instance.RequestAudio(m_AttackSfx)
                .WithPosition(transform.position)
                .Play();
        }
    }
}
