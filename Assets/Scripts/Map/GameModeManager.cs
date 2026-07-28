
using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
	public class GameModeManager: NetworkSingleton<GameModeManager>
	{
        private readonly List<PlayerController> m_Players = new();
        [Tooltip("失败场景Id")]
        [SerializeField] private int FailureSceneId;

        protected override void Awake()
        {
            base.Awake();
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnEnable()
        {
            EventManager.AddListener<Event.ActorDiedEvent>(OnActorDied);
            EventManager.AddListener<Event.ActorReviveEvent>(OnActorRevive);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Event.ActorDiedEvent>(OnActorDied);
            EventManager.RemoveListener<Event.ActorReviveEvent>(OnActorRevive);
        }

        private void OnActorDied(Event.ActorDiedEvent evt)
        {
            var actor = ActorManager.Instance.GetActor(evt.ActorId);
            if (actor == null || !actor.TryGetComponent<PlayerController>(out var player)) {
                return;
            }
            if(!m_Players.Contains(player)) {
                return;
            }
            UpdateState();
        }

        private void OnActorRevive(Event.ActorReviveEvent evt)
        {
            var actor = ActorManager.Instance.GetActor(evt.ActorId);
            if (actor == null || !actor.TryGetComponent<PlayerController>(out var player)) {
                return;
            }
            if (!m_Players.Contains(player)) {
                return;
            }
            UpdateState();
        }

        public void RegisterPlayer(PlayerController player)
        {
            if (!m_Players.Contains(player)) {
                m_Players.Add(player);
            }
            UpdateState();
        }

        public void UnregisterPlayer(PlayerController player)
        {
            if (m_Players.Contains(player)) {
                m_Players.Remove(player);
            }
            UpdateState();
        }

        private void UpdateState()
        {
            foreach(var player in m_Players) {
                if(!player.RuntimeData.IsDied) {
                    return;
                }
            }
            MapManager.Instance.EnterMap(FailureSceneId, true);
            EventManager.Broadcast(new Event.GameOverEvent { Success = false });
        }

        static public void ExitGame()
        {
            var mgr = GameNetworkManager.Instance;
            if (mgr != null && mgr.IsListening) {
                mgr.Disconnect();
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene("Boot");
        }
	}
}
