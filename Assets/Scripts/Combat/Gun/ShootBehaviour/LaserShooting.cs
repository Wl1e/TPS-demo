using UnityEngine;

namespace TPSDemo
{
	public class LaserShooting: ShootBehaviour
	{
        public override void Shoot(Vector3 dir, ulong clientId)
        {
            var bullet = CreateBullet(dir, clientId);
            bullet.OnShoot();
        }
    }
}
