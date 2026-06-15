using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace TPSDemo
{

    public class BulletAttacker : AttackerBase
    {
        [Tooltip("枪口位置")]
        public Transform Muzzle;
        [Tooltip("攻击间隔")]
        public float DelayBetweenShots;

        [Header("子弹相关")]
        [Tooltip("子弹预制体")]
        public NormalBulletController BulletPrefab;
        [Tooltip("子弹速度")]
        public float BulletSpeed;
        [Tooltip("子弹碰撞层")]
        public LayerMask HitLayerMask = -1;
        [Tooltip("伤害")]
        public float Damage;
        //public event Action<GameObject> OnTargetHit;

        [Tooltip("攻击范围")]
        public float MaxFireRange = 10f;

        [Header("资源")]
        [Tooltip("攻击音效")]
        public AudioClip ShootSfx;
        [Tooltip("枪口闪光")]
        public GameObject MuzzleFlashPrefab;

        float m_TimeLastFired = 0f;

        public override bool InFireRange(Transform target)
        {
            return (transform.position - target.position).sqrMagnitude <= MaxFireRange * MaxFireRange;
        }

        public override bool CanFire()
        {
            if (!enabled) {
                return false;
            }
            if (m_TimeLastFired + DelayBetweenShots > Time.time) {
                return false;
            }
            return true;
        }

        public override void Fire(Vector3 pos)
        {
            if(!IsServer) {
                return;
            }
            m_TimeLastFired = Time.time;

            NormalBulletController bullet = Instantiate(BulletPrefab, Muzzle.position, Quaternion.LookRotation(Vector3.Normalize(pos - Muzzle.position)));
            bullet.Owner = Owner.gameObject;
            bullet.Damage = Damage;
            bullet.Speed = BulletSpeed;
            bullet.HitLayerMask = HitLayerMask;

            var no = bullet.GetComponent<NetworkObject>();
            if (!no.IsSpawned) {
                no.SpawnWithOwnership(OwnerClientId);
            }

            bullet.OnShoot();
            WhenAttack();
            HandleShootClientRpc();
        }

        [ClientRpc]
        public void HandleShootClientRpc()
        {
            Director.Instance.RequestEffect(MuzzleFlashPrefab)
                .WithPosition(transform.position)
                .LookAt(transform.forward)
                .WithDuration(0.3f)
                .Create();
            
            Director.Instance.RequestAudio(ShootSfx)
                .WithPosition(transform.position)
                .Play();
        }
    }
}
