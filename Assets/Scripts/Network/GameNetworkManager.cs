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

        static public GameNetworkManager Instance { get; private set; }
        public ConnectionState m_ConnectionState { get; private set; }

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
            Application.targetFrameRate = FrameRate;
        }

        private async void Start()
        {
            OnClientConnectedCallback += OnClientConnected;
            OnClientDisconnectCallback += OnClientDisconnect;
            OnConnectionEvent += OnClientConnectionEvent;

            //if (UnityServices.Instance != null && UnityServices.Instance.State != ServicesInitializationState.Initialized) {
            //    await UnityServices.InitializeAsync();
            //}
            //if (!AuthenticationService.Instance.IsSignedIn) {
            //    AuthenticationService.Instance.SignInFailed += SignInFailed;
            //    AuthenticationService.Instance.SignedIn += SignedIn;
            //    if (m_ProfileName.Length <= 0) {
            //        m_ProfileName = "InitPrefileName";
            //    }
            //    AuthenticationService.Instance.SwitchProfile(m_ProfileName);
            //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //}
        }

        private void OnDestroy()
        {
            OnClientConnectedCallback -= OnClientConnected;
            OnClientDisconnectCallback -= OnClientDisconnect;
            OnConnectionEvent -= OnClientConnectionEvent;
            if (Instance == this) {
                Instance = null;
            }
        }

        public void ClientStart()
        {
            OnClientStarted += ClientStarted;
            OnClientStopped += ClientStopped;
            StartClient();
        }
        public void HostStart()
        {
            OnClientStarted += ClientStarted;
            OnClientStopped += ClientStopped;
            StartHost();
        }

        private void OnClientConnected(ulong clientId)
        {
            MessageLog.Log($"[{Time.realtimeSinceStartup}] Connected event invoked for Client-{clientId}.");
        }

        private void OnClientDisconnect(ulong clientId)
        {
            MessageLog.Log($"[{Time.realtimeSinceStartup}] Disconnected event invoked for Client-{clientId}.");
        }

        private void OnClientConnectionEvent(NetworkManager mgr, ConnectionEventData eventData)
        {
            MessageLog.Log($"[{Time.realtimeSinceStartup}] Connection event {eventData.EventType} for Client-{eventData.ClientId}.");
        }

        #region AuthenticationService

        //private void SignInFailed(RequestFailedException exp)
        //{
        //    AuthenticationService.Instance.SignInFailed -= SignInFailed;
        //    Debug.LogError($"Failed to sign in {m_ProfileName} anonymously: {exp}");
        //}

        //private void SignedIn()
        //{
        //    AuthenticationService.Instance.SignedIn -= SignedIn;
        //    Debug.Log($"Signed in anonymously with profile {m_ProfileName}");
        //}

        #endregion AuthenticationService

        public void Disconnect()
        {
            //if (m_CurSession != null && m_CurSession.State == SessionState.Connected) {
            //    m_CurSession.LeaveAsync();
            //    m_CurSession = null;
            //} else {
            Shutdown();
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
            if(IsHost) {
                m_ConnectionState = ConnectionState.Host;
            } else if(IsClient) {
                m_ConnectionState = ConnectionState.Connected;
            }
            MessageLog.Log($"Connected to session {m_SessionName}");
        }
    }
}
