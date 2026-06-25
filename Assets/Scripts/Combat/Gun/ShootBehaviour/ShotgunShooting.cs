using UnityEngine;

namespace TPSDemo
{
	public class ShotgunShooting: ShootBehaviour
    {
        [Tooltip("每发子弹数")]
        [SerializeField] private int M_BulletOneShoot = 8;
        [Tooltip("扩散尺寸，以标准圆为1")]
        [SerializeField] private float m_SpreadScale = 0.8f;

        public override void Shoot(Vector3 dir, ulong clientId)
        {
            CreateSpreadBullet(dir, clientId);
        }

        private void CreateSpreadBullet(Vector3 dir, ulong clientId)
        {
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            for (int i = 0; i < M_BulletOneShoot; i++) {
                Vector2 randomCircle = Random.insideUnitCircle * m_SpreadScale;
                Vector3 offset = right * randomCircle.x + Vector3.up * randomCircle.y;
                //var radius = Random.Range(0f, m_SpreadScale);

                //var rotation = Quaternion.AngleAxis(angle, dir);
                //var tiltRotation = Quaternion.AngleAxis(radius, right);
                var bullet = CreateBullet(dir + offset, clientId);
                //bullet.transform.Rotate(right, radius);
                //bullet.transform.Rotate(dir, angle);
                bullet.OnShoot();
            }
        }
    }
}
