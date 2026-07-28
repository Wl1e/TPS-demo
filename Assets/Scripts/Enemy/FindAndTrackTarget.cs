using UnityEngine;
using System;
using Unity.Netcode;

namespace TPSDemo
{
    // enemy的行为树有寻路逻辑，会卡住需要每次都调用的查找逻辑
    // 加上有一个短暂记忆Target的需求
    // 所以将这部分代码拿到逻辑层实现
    [Obsolete("废弃")]
	public class FindAndTrackTarget: NetworkBehaviour
	{
        [SerializeField] GameObjectEventChannel m_Channel;
        [SerializeField] float m_Duration = 1f;
        [SerializeField] LayerMask m_SeeLayer = -1;

        private GameObject m_Target = null;

        private float m_Time = -1f;

		public override void OnNetworkSpawn()
		{
            if(IsOwner) {
                m_Channel.Event += OnTargetChanged;
            }
		}

        public override void OnNetworkDespawn()
        {
            if (IsOwner) {
                m_Channel.Event -= OnTargetChanged;
            }
        }

        private void OnTargetChanged(GameObject target) => SetTarget(target);

		void Update()
		{
            if(m_Time <= 0f) {
                return;
            } else {
                bool canSee = AI.AITool.AgentSeeTarget(gameObject, m_Target, m_SeeLayer);
                if (canSee) {
                    Debug.Log("See Target, update LastTime");
                    m_Time = m_Duration;
                } else {
                    m_Time -= Time.deltaTime;
                }
                if (m_Time <= 0f) {
                    m_Channel.SendEventMessage(null);
                }
            }
		}

        public void SetTarget(GameObject target)
        {
            m_Target = target;
            if (m_Target != null) {
                m_Time = m_Duration;
            }
        }
	}
}
