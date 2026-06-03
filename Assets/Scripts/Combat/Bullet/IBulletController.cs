
using System;
using UnityEngine;

namespace TPSDemo
{

    public interface IBulletController
    {

    }

    public abstract class BulletController : MonoBehaviour, IBulletController
    {
        // 改为ID?
        public GameObject Owner;
        public float Damage;
        public float Speed;
        public LayerMask HitLayerMask = -1;

        protected Vector3 m_Velocity;

        public float MaxLifeTime = 5f;
        public bool DestroyOnHit = true;

        public Action<GameObject> OnHitTarget;

        // 音效需要一个全局对象创建和管理
        public AudioSource m_AudioSource;
        public GameObject HitFlashPrefab;
        public AudioClip HitSfx;

        protected virtual void Awake()
        {
            Destroy(gameObject, MaxLifeTime);
        }

        public abstract void OnShoot();
        protected void OnHit(RaycastHit hitInfo)
        {
            Damageable damageable = hitInfo.collider.GetComponent<Damageable>();
            if (damageable) {
                damageable.InflictDamage(Owner, Damage);
                OnHitTarget?.Invoke(hitInfo.collider.gameObject);
                EventManager.Broadcast(new Event.BulletHitTargetEvent { Attacker = Owner, Victim = damageable.gameObject });
            }
            if (DestroyOnHit) {
                Destroy(gameObject);
            }
            if (HitFlashPrefab) {
                var sfx = Instantiate(HitFlashPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                Destroy(sfx, 1f);
            }
            if (HitSfx) {
                m_AudioSource.PlayOneShot(HitSfx);
            }
        }

    }
}
