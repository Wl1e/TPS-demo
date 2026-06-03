using System;
using UnityEngine;

namespace TPSDemo
{

    public class Damageable : MonoBehaviour
    {
        public Health Health;
        public Action<GameObject, float> OnTakeDamaged;
        public void Start()
        {
            Health = GetComponent<Health>();
            if (!Health) {
                Health = GetComponentInParent<Health>();
            }
        }
        public void InflictDamage(GameObject attacker, float damage)
        {
            if (Health) {
                Health.TakeDamage(attacker, damage);
            }
        }
    }
}
