using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public interface IAttacker
    {
        public bool CanAttack();
        public float AttackRange {  get; }
        public bool InAttackRange(Transform target);
        public void Attack(Vector3 pos);
        public event Action OnAttack;
    }

    public class AttackerBase : NetworkBehaviour, IAttacker
    {
        public EnemyController Owner;

        // 攻击CD
        [SerializeField] protected float CD = 0f;
        protected float m_TimeLastAttack = 0f;

        [Tooltip("伤害")]
        [SerializeField] protected float m_Damage;

        [Tooltip("攻击范围")]
        [SerializeField] protected float m_AttackRange = 0f;

        public float AttackRange => m_AttackRange;

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
        public virtual bool InAttackRange(Transform target) =>
            (transform.position - target.position).sqrMagnitude <= m_AttackRange * m_AttackRange;

        public virtual void Attack(Vector3 pos) { }

        protected void WhenAttack()
        {
            m_TimeLastAttack = Time.time;
            
            OnAttack?.Invoke();
        }
    }
}
