using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    public class BulletAttacker : AttackerBase
    {
        [Tooltip("枪口位置")]
        public Transform Muzzle;

        [Header("子弹相关")]
        [Tooltip("子弹预制体")]
        public RifleBulletController BulletPrefab;
        [Tooltip("子弹速度")]
        public float BulletSpeed;
        [Tooltip("子弹碰撞层")]
        public LayerMask HitLayerMask = -1;
        
        //public event Action<GameObject> OnTargetHit;

        [Header("资源")]
        [Tooltip("攻击音效")]
        [SerializeField] private AudioClip m_ShootSfx;
        [Tooltip("枪口闪光")]
        [SerializeField] private GameObject m_MuzzleFlashPrefab;

        private void Awake()
        {
            Owner.AEPlayer.AddAudio("Attack", m_ShootSfx);
            Owner.AEPlayer.AddEffect("Attack", m_MuzzleFlashPrefab);
        }

        public override void Attack(Vector3 pos)
        {
            if(!IsServer) {
                return;
            }

            RifleBulletController bullet = Instantiate(BulletPrefab, Muzzle.position, Quaternion.LookRotation(Vector3.Normalize(pos - Muzzle.position)));
            bullet.Owner = Owner.gameObject;
            bullet.Damage = m_Damage;
            bullet.Speed = BulletSpeed;
            bullet.HitLayerMask = HitLayerMask;

            var no = bullet.GetComponent<NetworkObject>();
            if (!no.IsSpawned) {
                no.SpawnWithOwnership(OwnerClientId);
            }

            bullet.OnShoot();
            WhenAttack();
            Owner.AEPlayer.Play("Attack", 0.3f, transform.position, Quaternion.LookRotation(transform.forward));
            //HandleShootClientRpc();
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
