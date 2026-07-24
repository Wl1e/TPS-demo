using System;
using Unity.Netcode;
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
        /// <summary>
        /// 所属武器
        /// </summary>
        protected IWeapon m_Weapon;
        public GameObject Owner => m_Weapon.Owner;
        private Transform Muzzle;

        /// <summary>
        /// 子弹预制体
        /// </summary>
        public BulletController BulletPrefab => m_Weapon.Config.BulletPrefab;

        /// <summary>
        /// 弹速
        /// </summary>
        public float BulletSpeed => m_Weapon.Config.BulletSpeed;
        /// <summary>
        /// 子弹碰撞层
        /// </summary>
        public LayerMask HitLayerMask => m_Weapon.Config.HitLayerMask;
        /// <summary>
        /// 伤害
        /// </summary>
        public float Damage => m_Weapon.Config.Damage;

        /// <summary>
        /// 子弹碰撞目标时触发
        /// </summary>
        public event Action<GameObject> OnTargetHit;

        public void SetMuzzle(Transform muzzle) => Muzzle = muzzle;

        public void Initialize(IWeapon weapon) => m_Weapon = weapon;

        public abstract void Shoot(Vector3 dir, ulong clientId);

        /// <summary>
        /// 创造子弹(Server调用)
        /// </summary>
        /// <param name="dir"> 朝向 </param>
        /// <param name="clientId"></param>
        /// <returns></returns>
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
