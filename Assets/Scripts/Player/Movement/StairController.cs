using UnityEngine;

namespace TPSDemo
{
	public class StairController: MonoBehaviour
	{
        CharacterController m_Controller;
        [Tooltip("射线向前的偏移")]
        [SerializeField] private float m_RayOffset;
        [Tooltip("楼梯所在层")]
        [SerializeField] private LayerMask m_StairLayer;
        [Tooltip("楼梯高度范围")]
        [SerializeField] private Vector2 m_StairHeight;
        [Tooltip("可翻越障碍高度范围")]
        [SerializeField] private Vector2 m_ObstacleHeight;

        /// <summary>
        /// 碰撞点信息
        /// </summary>
        public RaycastHit HitInfo;

        /// <summary>
        /// 前方是否有阶梯
        /// </summary>
        public bool HasStair { get; private set; } = false;
        /// <summary>
        /// 前方是否有障碍物
        /// </summary>
        public bool HasObstacle { get; private set; } = false;

        /// <summary>
        /// 如果存在阶梯，是否为向上的阶梯
        /// </summary>
        public bool IsUp { get; private set; } = false;

		void Update()
		{
            Check();
        }

        private void Check()
        {
            if(!Physics.Raycast(GetRayOrigin(), Vector3.down, out HitInfo, 2 * m_Controller.height, m_StairLayer, QueryTriggerInteraction.Ignore)) {
                return;
            }

            float height = HitInfo.point.y - m_Controller.transform.position.y;
            HasObstacle = IsObstacle(height);
            HasStair = IsStair(Mathf.Abs(height));
            if(HasStair) {
                IsUp = height > 0;
            }
        }

        private bool IsObstacle(float height) => height >= m_ObstacleHeight.x && height <= m_ObstacleHeight.y;
        private bool IsStair(float height) => height >= m_StairHeight.x && height <= m_StairHeight.y;

        Vector3 GetRayOrigin() => m_Controller.transform.position + Vector3.up * (m_Controller.height) + transform.forward * m_RayOffset;

    }
}
