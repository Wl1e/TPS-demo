
using System;
using UnityEngine;

public class ProjectileBehaviour: ShootBehaviour
{
    public override void Shoot(Vector3 dir)
    {
        var bullet = CreateBullet(dir);
        bullet.OnShoot();
    }
}
