using UnityEngine;
using System.Collections;

namespace TPSDemo
{

    public class ClimbContoller : MonoBehaviour
    {
        CharacterController m_Controller;
        public float MaxClimbDeg = 90f;
        float m_MinClimbDot;

        public bool IsClimbing { get; }
        //public float ClimbRemainTime = 0.3f;
        //float m_LastClimbTime;

        public LayerMask CanClimbLayer;

        public Collision[] ClimbingColliders;
        public RaycastHit TopHitInfo;

        int m_ClimbContactCnt;
        Vector3 m_ClimbNormal;
        Vector3 m_LastNormal;

        public Vector3 LedgeNormal;
        public Vector3 ClimbNormal => m_ClimbNormal;
        public Vector3 LastNormal => m_LastNormal;

        [Tooltip("顶部射线高度")]
        public float TopRayHeight = 1f;
        [Tooltip("最近判断攀爬距离")]
        public float MinBodyClimbDistance = 0.09f;


        public bool CanClimb { get; private set; }
        public bool CanClimbTop { get; private set; }
        public bool CanLedge { get; private set; }

        void Awake()
        {
            // 有时候因为浮点数误差导致upDot计算有误，留0.001f
            m_MinClimbDot = Mathf.Cos(MaxClimbDeg * Mathf.Deg2Rad) + 0.001f;
        }

        private void Start()
        {
            m_Controller = GetComponentInParent<PlayerController>().CharacterController;
        }

        private void Update()
        {
            bool BodyCheck = Physics.Raycast(GetBodyRayOrigin(), m_Controller.transform.forward, m_Controller.radius + MinBodyClimbDistance, CanClimbLayer, QueryTriggerInteraction.Ignore);
            bool HeadCheck = Physics.Raycast(GetHeadRayOrigin(), m_Controller.transform.forward, m_Controller.radius + MinBodyClimbDistance, CanClimbLayer, QueryTriggerInteraction.Ignore);
            bool TopCheck = Physics.Raycast(GetTopRayOrigin(), Vector3.down, out TopHitInfo, TopRayHeight * 2, CanClimbLayer, QueryTriggerInteraction.Ignore);

            Debug.DrawLine(GetTopRayOrigin(), GetTopRayOrigin() + Vector3.down * (TopRayHeight * 2), Color.yellow);
            Debug.DrawLine(GetHeadRayOrigin(), GetHeadRayOrigin() + m_Controller.transform.forward * (m_Controller.radius + MinBodyClimbDistance), Color.red);
            Debug.DrawLine(GetBodyRayOrigin(), GetBodyRayOrigin() + m_Controller.transform.forward * (m_Controller.radius + MinBodyClimbDistance), Color.blue);

            CanClimb = BodyCheck && HeadCheck;
            CanLedge = BodyCheck && TopCheck && !HeadCheck;
        }

        Vector3 GetTopRayOrigin() => m_Controller.transform.position + Vector3.up * (m_Controller.height + TopRayHeight) +
                m_Controller.transform.forward * (m_Controller.radius + 0.15f);
        Vector3 GetHeadRayOrigin() => m_Controller.transform.position + Vector3.up * (m_Controller.height - 0.1f);
        Vector3 GetBodyRayOrigin() => m_Controller.transform.position + Vector3.up * (m_Controller.height * 0.5f);

        public void ClearData()
        {
            m_ClimbContactCnt = 0;
            m_ClimbNormal = Vector3.zero;
            m_LastNormal = Vector3.zero;
        }

        public void OnPlayerCollision(ControllerColliderHit hit)
        {
            ClearData();
            var collider = hit.collider;
            var layer = collider.gameObject.layer;
            if ((CanClimbLayer & (1 << layer)) == 0) {
                return;
            }

            var normal = hit.normal;
            float upDot = Vector3.Dot(normal, Vector3.up);
            if (upDot <= m_MinClimbDot) {
                m_ClimbContactCnt++;
                m_ClimbNormal += normal;
                m_LastNormal = normal;
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red);
            }

            CanClimb = (m_ClimbContactCnt != 0);
            //m_LastClimbTime = Time.time;
        }
    }
}
