using UnityEngine;
using System.Collections;

namespace TPSDemo
{
	public class ProjectileBulletController: RifleBulletController
	{
        [Tooltip("爆炸范围")]
        [SerializeField] private float m_Radius = 3.0f;

        //private DamageArea m_Area;

        protected override void OnHit(RaycastHit hitInfo)
        {
            var targets = Physics.OverlapSphere(hitInfo.point, m_Radius, HitLayerMask, QueryTriggerInteraction.Ignore);
            foreach (var target in targets) {
                if(!target.TryGetComponent<Damageable>(out var damageable)) {
                    continue;
                }
                var actor = target.GetComponentInParent<Actor>();
                if (!actor) {
                    continue;
                }
                if(Physics.Linecast(transform.position, actor.AimPoint.position, out var hitInfo2, -1, QueryTriggerInteraction.Ignore)) {
                    // collider和traget为hurtbox
                    if (hitInfo2.collider != target) {
                        continue;
                    }
                }

                damageable.InflictDamage(new DamageInfo { Attacker = Owner, Damage = Damage, Point = actor.AimPoint.position });
                OnHitTarget?.Invoke(target.gameObject);
                EventManager.Broadcast(new Event.BulletHitTargetEvent { Attacker = Owner, Victim = damageable.Owner });
            }

            PlayAE(hitInfo);

            if (IsServer && m_Config.DestroyOnHit) {
                NetworkObject.Despawn();
            }
        }
	}
}
