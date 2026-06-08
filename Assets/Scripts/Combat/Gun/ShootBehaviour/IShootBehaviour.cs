
using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace TPSDemo
{
    public interface IShootBehaviour
    {
        public GameObject Owner { get; }
        public void SetMuzzle(Transform muzzle);
        public void Initialize(IWeapon weapon);
        public void Shoot(Vector3 dir, ulong clientId);
        public event Action<GameObject> OnTargetHit;
    }

    public abstract class ShootBehaviour : MonoBehaviour, IShootBehaviour
    {
        IWeapon m_Weapon;
        public GameObject Owner => m_Weapon.Owner;
        public Transform Muzzle;

        // Bullet
        public BulletController BulletPrefab;
        public float BulletSpeed;
        public LayerMask HitLayerMask = -1;
        public float Damage;
        public event Action<GameObject> OnTargetHit;

        public void SetMuzzle(Transform muzzle) => Muzzle = muzzle;

        public void Initialize(IWeapon weapon)
        {
            m_Weapon = weapon;
        }

        public abstract void Shoot(Vector3 dir, ulong clientId);
        protected BulletController CreateBullet(Vector3 dir, ulong clientId)
        {
            var bullet = Instantiate(BulletPrefab, Muzzle.position, Quaternion.LookRotation(dir));
            bullet.Owner = Owner;
            bullet.Damage = Damage;
            bullet.Speed = BulletSpeed;
            bullet.HitLayerMask = HitLayerMask;
            bullet.OnHitTarget += OnBulletHit;

            var no = bullet.GetComponent<NetworkObject>();
            if (!no.IsSpawned) {
                no.SpawnWithOwnership(clientId);
            }
            return bullet;
        }
        protected virtual void OnBulletHit(GameObject obj)
        {
            OnTargetHit?.Invoke(obj);
        }
    }
}
