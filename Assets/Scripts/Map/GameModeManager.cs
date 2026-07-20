
using System.Collections.Generic;

namespace TPSDemo
{
	public class GameModeManager: NetworkSingleton<GameModeManager>
	{
        private readonly Dictionary<PlayerController, bool> m_Players = new();

        protected override void Awake()
        {
            base.Awake();
            EventManager.AddListener<Event.ActorDiedEvent>(OnActorDied);
        }

        private void OnActorDied(Event.ActorDiedEvent evt)
        {
            var actor = ActorManager.Instance.GetActor(evt.ActorId);
            if (actor == null || !actor.TryGetComponent<PlayerController>(out var player)) {
                return;
            }
            m_Players[player] = true;
            UpdateState();
        }

        public void RegisterPlayer(PlayerController player)
        {
            m_Players.TryAdd(player, false);
            UpdateState();
        }

        public void UnregisterPlayer(PlayerController player)
        {
            m_Players.Remove(player);
            UpdateState();
        }

        private void UpdateState()
        {
            foreach(var idDied in m_Players.Values) {
                if(!idDied) {
                    return;
                }
            }
            EventManager.Broadcast(new Event.GameOverEvent());
        }
	}
}
