using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    public class BulletAttacker : AttackerBase
    {
        [Tooltip("枪口位置")]
        public System.Collections.Generic.List<Transform> Muzzle;

        [Header("子弹相关")]
        [Tooltip("子弹预制体")]
        public RifleBulletController BulletPrefab;
        [Tooltip("子弹速度")]
        public float BulletSpeed;
        [Tooltip("子弹碰撞层")]
        public LayerMask HitLayerMask = -1;

        //public event Action<GameObject> OnTargetHit;

        [Header("资源")]
        [Tooltip("特效、音效持续时间")]
        [SerializeField] private float m_Duration = float.PositiveInfinity;
        [Tooltip("攻击音效")]
        [SerializeField] private AudioClip m_ShootSfx;
        [Tooltip("枪口闪光")]
        [SerializeField] private GameObject m_MuzzleFlashPrefab;
        [SerializeField] private AnimationClip m_AttackAnimation;

        private const string m_SFName = "BulletAttack";

        public override void OnNetworkSpawn()
        {
            Owner.AEPlayer.AddAudio(m_SFName, m_ShootSfx);
            Owner.AEPlayer.AddEffect(m_SFName, m_MuzzleFlashPrefab);
        }

        public override void Attack(Transform target)
        {
            if(!IsOwner) {
                return;
            }

            if(!CanAttack()) {
                return;
            }

            Vector3 dir = target.position - Owner.transform.position;
            dir.y = 0;

            Owner.transform.rotation = Quaternion.LookRotation(dir);

            //Owner.LookTo(dir);
            //if ((Owner.transform.forward.x - dir.x) >= 0.01 || (Owner.transform.forward.z - dir.z) >= 0.01) {
            //    return;
            //}

            //Owner.transform.rotation = Quaternion.LookRotation(dir);

            Vector3 pos = target.position;
            if (target.TryGetComponent<Actor>(out var actor)) {
                pos = actor.AimPoint.position;
            }

            foreach (Transform t in Muzzle) {
                SpawnBulletInMuzzle(t, pos);
                Owner.AEPlayer.Play(m_SFName, AudioSystem.AudioGroup.SFX, m_Duration, t.position, t.rotation);
            }

            WhenAttack();
            PlayerAE();
        }

        private void SpawnBulletInMuzzle(Transform muzzle, Vector3 targetPos)
        {
            RifleBulletController bullet = Instantiate(
                BulletPrefab, 
                muzzle.position,
                Quaternion.LookRotation(Vector3.Normalize(targetPos - muzzle.position))
            );
            bullet.Owner = Owner.gameObject;
            bullet.Damage = m_Damage;
            bullet.Speed = BulletSpeed;
            bullet.HitLayerMask = HitLayerMask;

            var no = bullet.GetComponent<NetworkObject>();
            if (!no.IsSpawned) {
                no.SpawnWithOwnership(OwnerClientId);
            }
            bullet.OnShoot();
        }

        protected void PlayerAE()
        {
            Owner.AnimatorController.Play("Attack");
        }


        //[ClientRpc]
        //public void HandleShootClientRpc()
        //{
        //    Director.Instance.RequestEffect(m_MuzzleFlashPrefab)
        //        .WithPosition(transform.position)
        //        .LookAt(transform.forward)
        //        .WithDuration(0.3f)
        //        .Create();
        //    Director.Instance.RequestAudio(m_ShootSfx)
        //        .WithPosition(transform.position)
        //        .Play();
        //}
    }
}
