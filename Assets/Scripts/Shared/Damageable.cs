using System;
using UnityEngine;

namespace TPSDemo
{

    public struct DamageInfo
    {
        public GameObject Attacker;
        public float Damage;
        public Vector3 Point;
    }

    public class Damageable : MonoBehaviour
    {
        public GameObject Owner;
        public Health Health;
        public Action<GameObject, float> OnTakeDamaged;
        public void Start()
        {
            Health = GetComponent<Health>();
            if (!Health) {
                Health = GetComponentInParent<Health>();
            }
        }
        public void InflictDamage(DamageInfo info)
        {
            float showDamage = info.Damage;
            if (Health) {
                showDamage = Health.TakeDamage(info);
            }
            Director.Instance.RequestDamageValue(
                info.Point + (info.Attacker.transform.position - info.Point).normalized * DamageValueUI.DamageValueOffset,
                showDamage,
                false
            );
        }
    }
}
