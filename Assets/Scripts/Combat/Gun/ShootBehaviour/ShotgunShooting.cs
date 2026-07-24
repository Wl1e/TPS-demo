using UnityEngine;

namespace TPSDemo
{
	public class ShotgunShooting: ShootBehaviour
    {
        public override void Shoot(Vector3 dir, ulong clientId)
        {
            CreateSpreadBullet(dir, clientId);
        }

        private void CreateSpreadBullet(Vector3 dir, ulong clientId)
        {
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            for (int i = 0; i < m_Weapon.Config.BulletsPerShot; i++) {
                Vector2 randomCircle = Random.insideUnitCircle * m_Weapon.Config.SpreadScale;
                Vector3 offset = right * randomCircle.x + Vector3.up * randomCircle.y;
                //var radius = Random.Range(0f, m_SpreadScale);

                //var rotation = Quaternion.AngleAxis(angle, dir);
                //var tiltRotation = Quaternion.AngleAxis(radius, right);
                var bullet = CreateBullet(dir.normalized + offset, clientId);
                //bullet.transform.Rotate(right, radius);
                //bullet.transform.Rotate(dir, angle);
                bullet.OnShoot();
            }
        }
    }
}
