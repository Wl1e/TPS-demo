using System;
using UnityEngine;

namespace TPSDemo
{
    /// <summary>
    /// 地图实例控制器 — 挂载在场景中的地图 GameObject 上
    /// 持有地图配置、刷怪点/入口点/边界等场景引用，提供进入/退出/完成钩子
    /// </summary>
    public class Map : MonoBehaviour
    {
        [Header("配置")]
        [Tooltip("地图配置")]
        [SerializeField] MapConfig m_Config;

        [Header("")]
        [Tooltip("玩家进入该地图时的起始位置")]
        [SerializeField] Transform m_EntryPoint;

        [Tooltip("完成地图的Objectives")]
        private System.Collections.Generic.List<Objective> m_Objectives;

        public Transform EntryPoint => m_EntryPoint;
        public MapConfig Config => m_Config;
        public MapState State => m_State;

        public event Action OnPlayerEnter;
        public event Action OnPlayerExit;
        public event Action OnMapComplete;

        MapState m_State;

        private void Awake()
        {
            MapManager.Instance.RegisterMap(this);
        }

        #region 公共方法

        /// <summary>玩家进入本地图</summary>
        public void OnEnter()
        {
            m_State = MapState.Active;
            OnPlayerEnter?.Invoke();
        }

        /// <summary>玩家离开本地图</summary>
        public void OnExit()
        {
            m_State = MapState.Idle;
            OnPlayerExit?.Invoke();
        }

        /// <summary>本地图目标达成</summary>
        public void OnComplete()
        {
            if (m_State == MapState.Completed) {
                return;
            }
            m_State = MapState.Completed;
            OnMapComplete?.Invoke();
        }

        #endregion

        #region 编辑器可视化

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (m_EntryPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(m_EntryPoint.position, 0.5f);
                Gizmos.DrawRay(m_EntryPoint.position, m_EntryPoint.forward * 1f);
            }
        }
#endif

        #endregion
    }
}
