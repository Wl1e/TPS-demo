
using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    public interface IBulletController
    {

    }

    public abstract class BulletController : NetworkBehaviour, IBulletController
    {
        // 改为ID?
        /// <summary>
        /// 攻击者
        /// </summary>
        public GameObject Owner;
        [Tooltip("伤害")]
        public float Damage;
        [Tooltip("飞行速度")]
        public float Speed;
        [Tooltip("攻击的层")]
        public LayerMask HitLayerMask = -1;

        protected Vector3 m_Velocity;

        [Tooltip("最大存活时间")]
        public float MaxLifeTime = 5f;
        [Tooltip("攻击后直接销毁")]
        public bool DestroyOnHit = true;

        public Action<GameObject> OnHitTarget;

        [Tooltip("特效")]
        public GameObject HitFlashPrefab;
        [Tooltip("攻击音效")]
        public AudioClip HitSfx;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) {
                Destroy(gameObject, MaxLifeTime);
            }
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
            if (IsServer && DestroyOnHit) {
                Destroy(gameObject);
            }
            Director.Instance.RequestEffect(HitFlashPrefab)
                .WithPosition(hitInfo.point)
                .LookAt(hitInfo.normal)
                .WithDuration(1)
                .Create();
            
            Director.Instance.RequestAudio(HitSfx).WithPosition(hitInfo.point).Play();
            
        }

    }
}
