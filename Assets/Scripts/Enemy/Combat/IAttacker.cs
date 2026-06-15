using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public interface IAttacker
    {
        public bool CanFire();
        public bool InFireRange(Transform target);
        public void Fire(Vector3 pos);
        public event Action OnAttack;
    }

    public class AttackerBase : NetworkBehaviour, IAttacker
    {
        public EnemyController Owner;
        public virtual bool CanFire() => false;
        public virtual bool InFireRange(Transform target) => false;
        public virtual void Fire(Vector3 pos) { }
        public event Action OnAttack;
        protected void WhenAttack()
        {
            OnAttack?.Invoke();
        }
    }
}
