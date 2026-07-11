
using System;
using Unity.Netcode;
using UnityEngine;
using static UniHumanoid.MuscleDebug;

namespace TPSDemo
{
    public class ProjectileShooting: ShootBehaviour
    {
        public override void Shoot(Vector3 dir, ulong clientId)
        {
            var bullet = CreateBullet(dir, clientId);
            bullet.OnShoot();
        }
    }
}
