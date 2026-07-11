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
    public class MapManager : NetworkSingleton<MapManager>
    {
        [Tooltip("所有地图")]
        [SerializeField] private List<MapConfig> m_MapConfigs;

        [SerializeField] private Map m_DefaultMap;

        private Map m_CurrentMap;
        public Map CurrentMap => m_CurrentMap;
        private Scene m_CurrentScene;
        public MapState State => m_CurrentMap != null ? m_CurrentMap.State : MapState.Idle;

        private bool m_SceneEventSubscribed = false;

        /// <summary>
        /// 同步给客户端：当前地图状态
        /// </summary>
        private readonly NetworkVariable<MapState> m_SyncedState = new(MapState.Idle);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsClient) {
                m_SyncedState.OnValueChanged += OnStateChanged;
            }

            if (!m_SceneEventSubscribed) {
                m_SceneEventSubscribed = true;
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
            }

            if (IsServer) {
                InitializeMap();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient) {
                m_SyncedState.OnValueChanged -= OnStateChanged;
            }
            if (m_SceneEventSubscribed) {
                m_SceneEventSubscribed = false;
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
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

        public void EnterMap(int mapId)
        {
            if (!IsServer) {
                return;
            }
            if(!CurrentMap.CanExit()) {
                EventManager.Broadcast(new Event.MessageLogEvent { Message = "无法离开场景，目标尚未完成" });
                return;
            }
            print($"Enter Map {mapId}");
            foreach (var config in m_MapConfigs) {
                if (config.MapId == mapId) {
                    EnterMap(config);
                    return;
                }
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

            if (string.IsNullOrEmpty(config.SceneName)) {
                Debug.LogError($"MapManager: Map '{config.MapName}' has no SceneName!");
                return;
            }

            LeaveMap();

            NetworkManager.SceneManager.LoadScene(config.SceneName, LoadSceneMode.Single);
        }

        private void OnSceneEvent(SceneEvent e)
        {
            //print($"SceneEventType: {e.SceneEventType} + SceneName: {e.SceneName} + ClientId: {e.ClientId}");
            if (e.SceneEventType == SceneEventType.UnloadComplete) {
                //NetworkManager.SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
            } else if (e.SceneEventType == SceneEventType.LoadComplete) {
                m_CurrentScene = e.Scene;
                var map = FindAnyObjectByType<Map>();
                // 如果m_CurrentMap等于map，代表是client进入触发
                if (IsServer && m_CurrentMap != map) {
                    InitializeMap();
                    TeleportToClientRpc(m_CurrentMap.EntryPoint.position, m_CurrentMap.EntryPoint.rotation);
                    //foreach(var actor in ActorManager.Instance.Actors.Values) {
                    //    if(actor.TryGetComponent<PlayerController>(out var player)) {
                            
                            //player.Movement.Teleport(, , Vector3.one);
                            
                        //}
                    //}
                } else if(IsOwner) {
                    EventManager.Broadcast(new Event.MessageLogEvent { Message = $"进入场景{map.Config.MapName}" });
                }
            } else if (e.SceneEventType == SceneEventType.Synchronize) {
            }
        }

        [ClientRpc]
        private void TeleportToClientRpc(Vector3 position, Quaternion rotation)
        {
            var player = PlayerDataProxy.Instance.GetPlayer();
            player.CharacterController.enabled = false;
            player.Movement.Teleport(position, rotation, Vector3.one);
            player.CharacterController.enabled = true;
        }

        /// <summary>完成当前地图</summary>
        public void CompleteCurrentMap()
        {
            if (!IsServer || m_CurrentMap == null)
                return;
            m_CurrentMap.OnComplete();
            m_SyncedState.Value = MapState.Completed;
            EventManager.Broadcast(new Event.MessageLogEvent { Message = $"完成: {m_CurrentMap.Config.MapName}" });
        }

        /// <summary>离开当前地图</summary>
        private void LeaveMap()
        {
            if (!IsServer || m_CurrentMap == null) {
                return;
            }
            m_CurrentMap.OnExit();
            if (m_SyncedState.Value == MapState.Active) {
                m_SyncedState.Value = MapState.Idle;
            }
            m_CurrentMap = null;
        }

        private void InitializeMap()
        {
            m_CurrentMap = FindAnyObjectByType<Map>();
            m_CurrentMap.OnEnter();
            m_SyncedState.Value = m_CurrentMap.State;
        }

        #endregion 地图切换（仅服务器）
    }
}
