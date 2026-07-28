using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    /// <summary>
    /// 地图实例控制器 — 挂载在场景中的地图 GameObject 上
    /// 持有地图配置、刷怪点/入口点/边界等场景引用，提供进入/退出/完成钩子
    /// </summary>
    public class Map : NetworkBehaviour
    {
        [Header("配置")]
        [Tooltip("地图配置")]
        [SerializeField] MapConfig m_Config;

        [Header("")]
        [Tooltip("玩家进入该地图时的起始位置")]
        [SerializeField] Transform m_EntryPoint;

        [Tooltip("完成地图的Objectives")]
        private readonly List<Objective> m_Objectives = new();

        public Transform EntryPoint => m_EntryPoint;
        public MapConfig Config => m_Config;
        public MapState State => m_State.Value;

        public event Action OnPlayerEnter;
        public event Action OnPlayerExit;
        public event Action OnMapComplete;

        private readonly NetworkVariable<MapState> m_State = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            foreach (var config in m_Config.ObjConfigs) {
                var obj = ObjectiveFactory.CreateObjective(config);
                m_Objectives.Add(obj);
                if (IsServer) {
                    obj.OnUpdate += UpdateObjective;
                    obj.OnCompleted += UpdateObjective;
                    obj.OnCompleted += _ => TryFinishMap();
                }
            }

            if(IsClient) {
                m_State.OnValueChanged += (pre, cur) => EventManager.Broadcast(
                    new Event.MapStateChangedEvent {
                        State = cur,
                    }
                );
            }
        }

        #region 公共方法

        /// <summary>
        /// 玩家进入本地图
        /// </summary>
        public void OnEnter()
        {
            var state = State;
            m_State.Value = MapState.Active;
            if(CheckAllObjective()) {
                m_State.Value = MapState.Completed;
            }
            Debug.Log($"Map {Config.MapId} state: {state} => {m_State}");
            OnPlayerEnter?.Invoke();
        }

        /// <summary>
        /// 玩家离开本地图
        /// </summary>
        public void OnExit()
        {
            m_State.Value = MapState.Idle;
            OnPlayerExit?.Invoke();
        }

        /// <summary>
        /// 本地图目标达成
        /// </summary>
        public void Complete()
        {
            if (State == MapState.Completed) {
                return;
            }
            m_State.Value = MapState.Completed;
            OnMapComplete?.Invoke();
        }

        /// <summary>
        /// 玩家是否能离开场景
        /// </summary>
        public bool CanExit()
        {
            return (Config.Type == MapType.Boot)
                || (Config.Type == MapType.Hub)
                || (Config.Type == MapType.Combat) && m_State.Value == MapState.Completed;
        }

        public List<ObjectiveProgress> GetObjectiveProgresses()
        {
            List<ObjectiveProgress> result = new();
            foreach (var obj in m_Objectives) {
                obj.GetProcess(out var progress);
                result.Add(progress);
            }
            return result;
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 检查当前地图的Objective
        /// </summary>
        private void UpdateObjective(Objective obj)
        {
            if (State == MapState.Completed) {
                return;
            }
            if (!m_Objectives.Contains(obj)) {
                return;
            }

            EventManager.Broadcast(new Event.MapObjectiveUpdateEvent());
            obj.GetProcess(out var progress);
            UpdateObjectiveClientRpc(m_Objectives.FindIndex(o => o.Id == obj.Id), progress);
            return;
        }

        private void TryFinishMap()
        {
            if (CheckAllObjective()) {
                Complete();
            }
        }

        private bool CheckAllObjective()
        {
            if (m_Objectives == null) {
                return true;
            }
            if (State == MapState.Completed) {
                return true;
            }

            bool objCompleted = true;
            foreach (var obj in m_Objectives) {
                if (!obj.IsCompleted) {
                    objCompleted = false;
                    break;
                }
            }

            EventManager.Broadcast(new Event.MapObjectiveUpdateEvent());

            return objCompleted;
        }

        [ClientRpc]
        private void UpdateObjectiveClientRpc(int idx, ObjectiveProgress progress)
        {
            if (!IsClient) {
                return;
            }
            var obj = m_Objectives[idx];
            obj.UpdateProcess(ref progress);
            EventManager.Broadcast(new Event.MapObjectiveUpdateEvent());
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
