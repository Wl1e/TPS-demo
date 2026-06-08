using UnityEngine;
using System.Collections;
using System;

namespace TPSDemo
{
    public class LaserBulletController : BulletController
    {
        public float MaxDistance = 100f;
        public LayerMask HitLayer;

        public float LaserWidth = 0.1f;
        [ColorUsage(true, true)]
        public Color LaserColor = Color.white;
        RaycastHit m_Info;

        public Transform LaserOrigin;
        public Transform Laser;

        protected void Awake()
        {
            transform.localScale = new Vector3(LaserWidth, LaserWidth, 1);
            Laser.GetComponent<Renderer>().material.SetColor("_EmissionColor", LaserColor);
        }

        public override void OnShoot()
        {
            if(!IsServer) {
                return;
            }
            var forward = transform.forward;
            var distance = MaxDistance;
            var end = forward * MaxDistance + transform.position;
            if (
                Physics.Raycast(transform.position, forward, out m_Info,
                MaxDistance, HitLayer, QueryTriggerInteraction.Ignore)
            ) {
                end = m_Info.point;
                distance = m_Info.distance;
                OnHit(m_Info);
            }
            transform.localScale = new Vector3(LaserWidth, LaserWidth, distance);

        }
    }
}
