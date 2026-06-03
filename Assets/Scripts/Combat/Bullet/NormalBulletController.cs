using System;
using UnityEngine;

namespace TPSDemo
{
    public class NormalBulletController : BulletController
    {
        public Transform Root;
        public Transform Tip;

        float DetecionRadius = 0.0019f;

        bool m_WillHit = false;
        RaycastHit m_HitInfo;

        RaycastHit[] m_Results = new RaycastHit[10];
        // 如果碰撞后马上销毁则不需要
        bool m_IsFinished = false;

        void Update()
        {
            if (m_IsFinished) {
                return;
            }
            if (!m_WillHit) {
                CheckHit();
            }
            UpdateMovement();
        }

        public override void OnShoot()
        {
            m_Velocity = Speed * transform.forward;
        }

        void UpdateMovement()
        {
            if (m_WillHit) {
                m_Velocity = Vector3.zero;
                transform.position = m_HitInfo.point;
                OnHit(m_HitInfo);
                m_IsFinished = true;
                return;
            }
            transform.position += m_Velocity * Time.deltaTime;
        }

        // 向前提前预测碰撞点
        void CheckHit()
        {
            float minDistance = float.PositiveInfinity;
            int count = Physics.SphereCastNonAlloc(
                    Tip.position, DetecionRadius, transform.forward,
                    m_Results, Speed * Time.deltaTime,
                    HitLayerMask, QueryTriggerInteraction.Ignore);
            if (count != 0) {
                m_WillHit = true;
                for (int i = 0; i < count; i++) {
                    if (minDistance > m_Results[i].distance) {
                        minDistance = m_Results[i].distance;
                        m_HitInfo = m_Results[i];
                    }
                }
            }
        }
    }
}
