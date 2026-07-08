using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public abstract class AttackerBase : NetworkBehaviour
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
            //print("distance: " + Mathf.Sqrt(distSqr));
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
