using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace TPSDemo
{
    public class GameNetworkManager : NetworkManager
    {
        public enum ConnectionState
        {
            None = 0,
            Connecting,
            Connected,
            Failed,
            Host,
        }

        public int FrameRate = 60;
        public bool EnableVSync = false;

        private ISession m_CurSession;
        private string m_SessionName;
        private string m_ProfileName;
        private Task m_SessionTask;

        private List<MessageLog> m_MessageLogs = new List<MessageLog>();

        public GameNetworkManager Instance { get; private set; }
        public ConnectionState m_ConnectionState { get; private set; }

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            OnClientConnectedCallback += OnClientConnected;
            OnClientDisconnectCallback += OnClientDisconnect;
            OnConnectionEvent += OnClientConnectionEvent;
            if (UnityServices.Instance != null && UnityServices.Instance.State != ServicesInitializationState.Initialized) {
                await UnityServices.InitializeAsync();
            }
            if (!AuthenticationService.Instance.IsSignedIn) {
                AuthenticationService.Instance.SignInFailed += SignInFailed;
                AuthenticationService.Instance.SignedIn += SignedIn;
                if (m_ProfileName.Length <= 0) {
                    m_ProfileName = "InitPrefileName";
                }
                AuthenticationService.Instance.SwitchProfile(m_ProfileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        private void OnDestroy()
        {
            OnClientConnectedCallback -= OnClientConnected;
            OnClientDisconnectCallback -= OnClientDisconnect;
            OnConnectionEvent -= OnClientConnectionEvent;
        }

        private void OnClientConnected(ulong clientId)
        {
            LogMessage($"[{Time.realtimeSinceStartup}] Connected event invoked for Client-{clientId}.");
        }

        private void OnClientDisconnect(ulong clientId)
        {
            LogMessage($"[{Time.realtimeSinceStartup}] Disconnected event invoked for Client-{clientId}.");
        }

        private void OnClientConnectionEvent(NetworkManager mgr, ConnectionEventData eventData)
        {
            LogMessage($"[{Time.realtimeSinceStartup}] Connection event {eventData.EventType} for Client-{eventData.ClientId}.");
        }

        #region AuthenticationService

        private void SignInFailed(RequestFailedException exp)
        {
            AuthenticationService.Instance.SignInFailed -= SignInFailed;
            Debug.LogError($"Failed to sign in {m_ProfileName} anonymously: {exp}");
        }

        private void SignedIn()
        {
            AuthenticationService.Instance.SignedIn -= SignedIn;
            Debug.Log($"Signed in anonymously with profile {m_ProfileName}");
        }

        #endregion AuthenticationService

        private void OnDrawDAHostGUI()
        {
            if (GUILayout.Button("Start Host")) {
                OnClientStarted += ClientStarted;
                OnClientStopped += ClientStopped;
                StartHost();
                Cursor.lockState = CursorLockMode.Locked;
            }
            if (GUILayout.Button("Start Client")) {
                OnClientStarted += ClientStarted;
                OnClientStopped += ClientStopped;
                StartClient();
            }
        }

        private void OnUpdateGUIDisconnected()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 800));
            GUILayout.Label("Session Name", GUILayout.Width(100));

            var connectionType = m_ConnectionState;
            if (NetworkConfig.NetworkTopology == NetworkTopologyTypes.ClientServer &&
                connectionType != ConnectionState.Host) {
                connectionType = ConnectionState.Host;
            }

            OnDrawDAHostGUI();

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(10, Display.main.renderingHeight - 40, Display.main.renderingWidth - 10, 30));
            var scenesPreloaded = new System.Text.StringBuilder();
            scenesPreloaded.Append("Scenes Preloaded: ");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                scenesPreloaded.Append($"[{scene.name}]");
            }
            GUILayout.Label(scenesPreloaded.ToString());
            GUILayout.EndArea();
        }

        private void OnGUI()
        {
            var yAxisOffset = 10;
            if (m_ConnectionState == ConnectionState.None) {
                yAxisOffset = 80;
                OnUpdateGUIDisconnected();
            } else if (m_ConnectionState == ConnectionState.Connected) {
                yAxisOffset = 40;
                OnUpdateGUIConnected();
            }

            // print message
        }

        private void OnUpdateGUIConnected()
        {
            if (CMBServiceConnection) {
                GUILayout.BeginArea(new Rect(10, 10, 800, 800));
                GUILayout.Label($"Session: {m_SessionName}");
                GUILayout.EndArea();
            } else {
                GUILayout.BeginArea(new Rect(10, 10, 800, 800));
                if (DistributedAuthorityMode) {
                    GUILayout.Label($"DAHosted Session");
                } else {
                    GUILayout.Label($"Client-Server Session");
                }

                GUILayout.EndArea();
            }

            GUILayout.BeginArea(new Rect(Display.main.renderingWidth - 160, 10, 150, 80));
            if (GUILayout.Button("Disconnect")) {
                if (m_CurSession != null && m_CurSession.State == SessionState.Connected) {
                    m_CurSession.LeaveAsync();
                    m_CurSession = null;
                } else {
                    Shutdown();
                }
            }
            GUILayout.EndArea();
        }

        private void ClientStopped(bool isHost)
        {
            OnClientStopped -= ClientStopped;
            m_ConnectionState = ConnectionState.None;
            m_CurSession = null;
        }

        private void ClientStarted()
        {
            OnClientStarted -= ClientStarted;
            m_ConnectionState = ConnectionState.Connected;
            LogMessage($"Connected to session {m_SessionName}");
        }

        private void Update()
        {
            if (m_MessageLogs.Count == 0) {
                return;
            }
            for (int i = m_MessageLogs.Count - 1; i >= 0; i--) {
                if (m_MessageLogs[i].ExpirationTime < Time.realtimeSinceStartup) {
                    m_MessageLogs.RemoveAt(i);
                }
            }
        }

        #region MessageLog

        private class MessageLog
        {
            public string Message { get; private set; }
            public float ExpirationTime { get; private set; }

            public MessageLog(string msg, float timeToLive)
            {
                Message = msg;
                ExpirationTime = Time.realtimeSinceStartup + timeToLive;
            }
        }

        public void LogMessage(string msg, float liveTime = 10f)
        {
            if (m_MessageLogs.Count > 0) {
                m_MessageLogs.Insert(0, new MessageLog(msg, liveTime));
            } else {
                m_MessageLogs.Add(new MessageLog(msg, liveTime));
            }
            Debug.Log(msg);
        }

        #endregion MessageLog
    }
}
