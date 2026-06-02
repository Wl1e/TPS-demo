using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[RequireComponent (typeof(AudioSource))]
public class BulletAttacker: AttackerBase
{
    public Transform Muzzle;
    public float DelayBetweenShots;

    // Bullet
    public NormalBulletController BulletPrefab;
    public float BulletSpeed;
    public LayerMask HitLayerMask = -1;
    public float Damage;
    //public event Action<GameObject> OnTargetHit;

    public float MaxFireRange = 10f;

    public AudioSource m_AudioSource;
    public GameObject MuzzleFlashPrefab;
    public AudioClip ShootSfx;

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
        m_TimeLastFired = Time.time;

        NormalBulletController bullet = Instantiate(BulletPrefab, Muzzle.position, Quaternion.LookRotation(Vector3.Normalize(pos - Muzzle.position)));
        bullet.Owner = Owner.gameObject;
        bullet.Damage = Damage;
        bullet.Speed = BulletSpeed;
        bullet.HitLayerMask = HitLayerMask;
        //bullet.OnHitTarget += OnBulletHit;
        bullet.OnShoot();
        HandleShoot();
    }

    public void HandleShoot()
    {
        if (MuzzleFlashPrefab) {
            var sfx = Instantiate(MuzzleFlashPrefab, Muzzle.position, Muzzle.rotation, Muzzle);
            Destroy(sfx, 1f);
        }
        if (ShootSfx) {
            m_AudioSource.PlayOneShot(ShootSfx);
        }
    }
}
