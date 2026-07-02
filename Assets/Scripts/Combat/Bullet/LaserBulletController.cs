using UnityEngine;

namespace TPSDemo
{
    public class LaserBulletController : BulletController
    {
        [Header("激光子弹属性")]
        [Tooltip("最远距离")]
        public float MaxDistance = 100f;
        [Tooltip("激光颜色")]
        [ColorUsage(true, true)]
        public Color LaserColor = Color.white;
        RaycastHit m_Info;

        [Tooltip("激光起点")]
        public Transform LaserOrigin;
        public Transform Laser;

        protected override void Awake()
        {
            base.Awake();
            Laser.GetComponent<Renderer>().material.SetColor("_EmissionColor", LaserColor);
        }

        public override void OnShoot()
        {
            if(!IsServer) {
                return;
            }
            var forward = transform.forward;
            var distance = MaxDistance;
            if (
                Physics.Raycast(transform.position, forward, out m_Info,
                MaxDistance, HitLayerMask, QueryTriggerInteraction.Ignore)
            ) {
                distance = m_Info.distance;
                OnHit(m_Info);
            }
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, distance);
        }
    }
}
