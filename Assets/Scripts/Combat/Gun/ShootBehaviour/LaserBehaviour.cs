using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;

namespace TPSDemo
{
	public class LaserBehaviour: ShootBehaviour
	{
        public override void Shoot(Vector3 dir, ulong clientId)
        {
            var bullet = CreateBullet(dir, clientId);
            bullet.OnShoot();
        }
    }
}
