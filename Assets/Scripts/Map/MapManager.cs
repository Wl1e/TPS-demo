using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSDemo
{
    /// <summary>
    /// 地图管理器（网络同步） — 加载/卸载地图场景，管理地图状态切换
    /// 仅服务器可更改, 客户端通过 NetworkVariable 自动同步
    /// </summary>
    public class MapManager : NetworkSingleton<MapManager>
    {
        [Tooltip("所有地图")]
        [SerializeField] private List<MapConfig> m_MapConfigs;

        [SerializeField] private Map m_DefaultMap;

        private Map m_CurrentMap;
        public Map CurrentMap => m_CurrentMap;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            base.OnNetworkDespawn();
        }

        #region 客户端回调

        private void OnStateChanged(MapState oldState, MapState newState)
        {
            EventManager.Broadcast(
                new Event.MapStateChangedEvent {
                    State = newState,
                });
        }

        #endregion 客户端回调

        #region 地图切换（仅服务器）

        public void EnterMap(int mapId, bool force = false)
        {
            if (!IsServer) {
                return;
            }
            if(CurrentMap && !CurrentMap.CanExit() && !force) {
                EventManager.Broadcast(new Event.MessageLogEvent { Message = "无法离开场景，目标尚未完成" });
                return;
            }
            Debug.Log($"Enter Map {mapId}");
            foreach (var config in m_MapConfigs) {
                if (config.MapId == mapId) {
                    EnterMap(config);
                    return;
                }
            }
        }

        public void TeleportToSpawnPoint()
        {
            if (m_CurrentMap == null) {
                return;
            }
            var player = GameNetworkManager.Instance.LocalClient.PlayerObject.GetComponent<PlayerController>();

            if (m_CurrentMap.EntryPoint != null) {
                player.CharacterController.enabled = false;
                player.Movement.Teleport(m_CurrentMap.EntryPoint.position, m_CurrentMap.EntryPoint.rotation, Vector3.one);
                player.CharacterController.enabled = true;
            }
        }

        /// <summary>
        /// 切换到指定地图配置的场景（仅服务器调用
        /// </summary>
        public void EnterMap(MapConfig config)
        {
            if (!IsServer) {
                return;
            }

            if (m_CurrentMap != null && m_CurrentMap.Config.MapId == config.MapId) {
                Debug.Log($"已经在Map{m_CurrentMap.Config.MapId}中了");
                return;
            }

            if (string.IsNullOrEmpty(config.SceneName)) {
                Debug.LogError($"MapManager: Map '{config.MapName}' has no SceneName!");
                return;
            }

            LeaveMap();

            NetworkManager.SceneManager.LoadScene(config.SceneName, LoadSceneMode.Single);
        }

        private void OnSceneEvent(SceneEvent e)
        {
            //Log.Debug($"SceneEventType: {e.SceneEventType} + SceneName: {e.SceneName} + ClientId: {e.ClientId}");
            if (e.SceneEventType == SceneEventType.UnloadComplete) {
                //NetworkManager.SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
            } else if (e.SceneEventType == SceneEventType.LoadComplete) {
                var map = FindAnyObjectByType<Map>();
                // 如果m_CurrentMap等于map，代表是client进入触发
                if (IsServer && m_CurrentMap != map) {
                    m_CurrentMap = map;
                    InitializeMap();
                    //TeleportToClientRpc(m_CurrentMap.EntryPoint.position, m_CurrentMap.EntryPoint.rotation);
                }
                if(IsClient) {
                    m_CurrentMap = map;
                    var player = PlayerDataProxy.Instance.GetPlayer();
                    player.Movement.Teleport(m_CurrentMap.EntryPoint.position, m_CurrentMap.EntryPoint.rotation, Vector3.one);
                    EventManager.Broadcast(new Event.MessageLogEvent { Message = $"进入地图{m_CurrentMap.Config.MapName}" });
                    EventManager.Broadcast(new Event.MapChangeEvent { NewMapId = m_CurrentMap.Config.MapId });
                }
            } else if (e.SceneEventType == SceneEventType.Load) {
                if(IsClient) {
                    StartCoroutine(UpdateProcess(e.AsyncOperation));
                }
            }
        }

        /// <summary>离开当前地图</summary>
        private void LeaveMap()
        {
            if (!IsServer || m_CurrentMap == null) {
                return;
            }
            m_CurrentMap.OnMapComplete -= CurrentMapCompleted;
            m_CurrentMap.OnExit();
            m_CurrentMap = null;
        }

        private void InitializeMap()
        {
            m_CurrentMap.OnEnter();
            m_CurrentMap.OnMapComplete += CurrentMapCompleted;
        }

        private void CurrentMapCompleted()
        {
            EventManager.Broadcast(new Event.MessageLogEvent { Message = $"完成: {m_CurrentMap.Config.MapName}" });
        }

        #endregion 地图切换（仅服务器）

        #region UI
        private IEnumerator UpdateProcess(AsyncOperation asyncOperation)
        {
            while (!asyncOperation.isDone) {
                EventManager.Broadcast(
                    new Event.MapLoadProgressEvent {
                        MapId = 0,
                        Progress = asyncOperation.progress,
                        IsCompleted = asyncOperation.isDone
                    });
                yield return null;
            }
            EventManager.Broadcast(
                    new Event.MapLoadProgressEvent {
                        MapId = 0,
                        Progress = asyncOperation.progress,
                        IsCompleted = asyncOperation.isDone
                    });
        }
        #endregion
    }
}
