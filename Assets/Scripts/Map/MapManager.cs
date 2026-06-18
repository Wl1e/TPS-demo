using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.STP;

namespace TPSDemo
{
    /// <summary>
    /// 地图管理器（网络同步） — 加载/卸载地图场景，管理地图状态切换
    /// 仅服务器可更改, 客户端通过 NetworkVariable 自动同步
    /// </summary>
    public class MapManager: NetworkSingleton<MapManager>
    {
        [Tooltip("所有地图")]
        [SerializeField] List<MapConfig> m_MapConfigs;
        private Map m_CurrentMap;
        public Map CurrentMap => m_CurrentMap;
        private Scene m_CurrentScene;
        public MapState State => m_CurrentMap != null ? m_CurrentMap.State : MapState.Idle;

        /// <summary>同步给客户端：当前地图状态</summary>
        NetworkVariable<int> m_SyncedState = new((int)MapState.Idle);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient)
            {
                //m_SyncedMapId.OnValueChanged += OnMapIdChanged;
                m_SyncedState.OnValueChanged += OnStateChanged;
                NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            }
        }

        #region 客户端回调

        //void OnMapIdChanged(int oldId, int newId)
        //{
        //    if (newId < 0) return;
        //    foreach (var config in m_MapConfigs) {
        //        if (config != null && config.MapId == newId) {
        //            break;
        //        }
        //    }
        //}

        void OnStateChanged(int oldState, int newState)
        {
            EventManager.Broadcast(
                new Event.MapStateChangedEvent {
                State = (MapState)newState,
            });
        }

        #endregion

        #region 地图切换（仅服务器）

        public void EnterMap(int mapId)
        {
            print("EnterMap");
            if (!IsServer) {
                return;
            }
            foreach (var config in m_MapConfigs) {
                if (config.MapId == mapId) {
                    EnterMap(config);
                    return;
                }
            }
        }

        /// <summary>
        /// 注册Map
        /// </summary>
        public void RegisterMap(Map map)
        {
            if (map == null) {
                return;
            }
            m_CurrentMap = map;
            OnMapEntered();
        }

        /// <summary>
        /// 切换到指定地图配置的场景（仅服务器调用
        /// </summary>
        public void EnterMap(MapConfig config)
        {
            if (!IsServer) {
                return;
            }

            if (string.IsNullOrEmpty(config.SceneName)) {
                Debug.LogError($"MapManager: Map '{config.MapName}' has no SceneName!");
                return;
            }

            LeaveMap();

            print("Start Change Map");
            //NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            NetworkManager.SceneManager.LoadScene(config.SceneName, LoadSceneMode.Single);
        }

        private void OnMapEntered()
        {
            m_CurrentMap.OnEnter();
        }

        void OnSceneEvent(SceneEvent e)
        {
            if (e.SceneEventType == SceneEventType.UnloadComplete) {
                //NetworkManager.SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
            } else if (e.SceneEventType == SceneEventType.LoadComplete) {
                m_CurrentScene = e.Scene;
                m_CurrentMap.OnEnter();
                m_SyncedState.Value = (int)MapState.Active;
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
                //EventManager.Broadcast(new Event.MessageLogEvent { Message = $"进入: {config.MapName}" });
            }
        }

        /// <summary>完成当前地图</summary>
        public void CompleteCurrentMap()
        {
            if (!IsServer || m_CurrentMap == null) return;
            m_CurrentMap.OnComplete();
            m_SyncedState.Value = (int)MapState.Completed;
            EventManager.Broadcast(new Event.MessageLogEvent { Message = $"完成: {m_CurrentMap.Config.MapName}" });
        }

        /// <summary>离开当前地图</summary>
        private void LeaveMap()
        {
            if (!IsServer || m_CurrentMap == null) {
                return;
            }
            m_CurrentMap.OnExit();
            if (m_SyncedState.Value == (int)MapState.Active) {
                m_SyncedState.Value = (int)MapState.Idle;
            }
            m_CurrentMap = null;
        }

        #endregion
    }
}
