using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public interface IAttacker
    {
        public bool CanAttack();

        public Vector2 AttackRange { get; }
        public bool InAttackRange(Transform target);
        public void Attack(Transform target);
        public event Action OnAttack;
    }

    public abstract class AttackerBase : NetworkBehaviour, IAttacker
    {
        public EnemyController Owner;

        // 攻击CD
        [SerializeField] protected float CD = 0f;
        protected float m_TimeLastAttack = 0f;

        [Tooltip("伤害")]
        [SerializeField] protected float m_Damage;

        [Tooltip("攻击范围")]
        [SerializeField] protected Vector2 m_AttackRange = Vector2.zero;

        public Vector2 AttackRange => m_AttackRange;

        public event Action OnAttack;

        public virtual bool CanAttack() => CheckCD();

        protected virtual bool CheckCD()
        {
            if (!enabled) {
                return false;
            }
            if (m_TimeLastAttack + CD > Time.time) {
                return false;
            }
            return true;
        }

        public virtual bool InAttackRange(Transform target)
        {
            float distSqr = (transform.position - target.position).sqrMagnitude;
            return distSqr >= m_AttackRange.x * m_AttackRange.x && distSqr <= m_AttackRange.y * m_AttackRange.y;
        }

        public abstract void Attack(Transform target);

        protected void WhenAttack()
        {
            m_TimeLastAttack = Time.time;
            OnAttack?.Invoke();
        }
    }
}
