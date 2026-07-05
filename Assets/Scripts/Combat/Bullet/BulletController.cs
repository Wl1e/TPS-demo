
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public abstract class BulletController : NetworkBehaviour
    {
        [Header("子弹基本属性")]
        [Tooltip("配置文件")]
        [SerializeField] protected BulletItemData m_Config;

        #region 由武器设置

        // 改为ID?
        /// <summary>
        /// 攻击者
        /// </summary>
        [HideInInspector] public GameObject Owner;
        /// <summary>
        /// 伤害
        /// </summary>
        [HideInInspector] public float Damage;
        /// <summary>
        /// 飞行速度
        /// </summary>
        [HideInInspector] public float Speed;
        /// <summary>
        /// 攻击的层
        /// </summary>
        [HideInInspector] public LayerMask HitLayerMask = -1;

        #endregion

        protected Vector3 m_Velocity;

        protected AudioAndEffectPlayGlobal m_AudioAndEffectPlayGlobal;

        /// <summary>
        /// 命中目标时回调
        /// </summary>
        public Action<GameObject> OnHitTarget;


        protected virtual void Awake()
        {
            m_AudioAndEffectPlayGlobal = GetComponent<AudioAndEffectPlayGlobal>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) {
                StartCoroutine(DespawnCoroutine());
            }
            foreach(var effect in m_Config.HitImpactPrefab) {
                m_AudioAndEffectPlayGlobal.AddEffect(effect.Tag, effect.Effect);
            }
            m_AudioAndEffectPlayGlobal.AddAudio("SFX", m_Config.HitSfx);
            m_AudioAndEffectPlayGlobal.AddEffect("BulletHole", m_Config.BulletHolePrefab);
        }

        /// <summary>
        /// 子弹开始更新
        /// </summary>
        public abstract void OnShoot();

        /// <summary>
        /// 命中时调用，逻辑包括（计算伤害、销毁子弹、播放音/特效、广播）
        /// </summary>
        /// <param name="hitInfo"> 命中信息 </param>
        protected virtual void OnHit(RaycastHit hitInfo)
        {
            Damageable damageable = hitInfo.collider.GetComponent<Damageable>();
            if (damageable) {
                damageable.InflictDamage(new DamageInfo { Attacker = Owner, Damage = Damage, Point = hitInfo.point });
                OnHitTarget?.Invoke(hitInfo.collider.gameObject);
                EventManager.Broadcast(new Event.BulletHitTargetEvent { Attacker = Owner, Victim = damageable.Owner });
            }

            PlayAE(hitInfo);

            //if (effectIdx >= 0) {
            //    Director.Instance.RequestEffect(.Effect)
            //        .WithPosition(hitInfo.point)
            //        .LookAt()
            //        .Create();
            //}
            //Director.Instance.RequestAudio(m_Config.HitSfx).WithPosition(hitInfo.point).Play();

            if (IsServer && m_Config.DestroyOnHit) {
                NetworkObject.Despawn();
            }
        }

        private IEnumerator DespawnCoroutine()
        {
            yield return new WaitForSeconds(m_Config.MaxLifeTime);
            NetworkObject.Despawn();
        }

        protected void PlayAE(RaycastHit hitInfo)
        {
            string tag = hitInfo.collider.gameObject.tag;
            var effectIdx = m_Config.HitImpactPrefab.FindIndex(e => e.Tag == tag);
            if (effectIdx < 0) {
                tag = "Default";
            }
            Vector3 up = Vector3.up;
            if (hitInfo.normal == Vector3.up) {
                up = Vector3.Cross(hitInfo.normal, (hitInfo.point - Owner.transform.position).normalized);
            }
            Quaternion rotation = Quaternion.LookRotation(hitInfo.normal, up);
            m_AudioAndEffectPlayGlobal.Play(tag, m_Config.HitImpactDuration, hitInfo.point, rotation);
            m_AudioAndEffectPlayGlobal.Play("SFX", m_Config.HitSfxDuration, hitInfo.point, Quaternion.identity);
            // 打怪身上不要弹孔
            // 这样不严谨，或许应该判断可以留单孔的位置，
            // 或许要给物体添加脚本
            if (!hitInfo.collider.gameObject.CompareTag("Enemy")) {
                m_AudioAndEffectPlayGlobal.Play("BulletHole", m_Config.BulletHoleDuration, hitInfo.point, rotation);
            }
        }
    }
}
