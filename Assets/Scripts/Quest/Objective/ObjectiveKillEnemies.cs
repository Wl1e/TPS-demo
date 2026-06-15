using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

namespace TPSDemo
{

    using ActorDiedEvent = Event.ActorDiedEvent;

    public class ObjectiveKillEnemies : Objective
    {
        public GameObject Enemy;
        public int Cur;
        public int Count;

        public override void Initialize(ObjectiveConfig config)
        {
            if(config is not ObjectiveKillEnemiesConfig trueConfig) {
                return;
            }
            Enemy = trueConfig.Target;
            Count = trueConfig.Count;
            Cur = 0;
            EventManager.AddListener<ActorDiedEvent>(OnObjDied);
        }

        public override void Destroy()
        {
            EventManager.RemoveListener<ActorDiedEvent>(OnObjDied);
        }

        void OnObjDied(ActorDiedEvent evt)
        {
            if(!Enemy.TryGetComponent<Actor>(out var actor)) {
                Debug.LogError("[ObjectiveKillEnemies] Enemy dont have Actor Component");
                return;
            }
            if (evt.ActorId != actor.Id || evt.AttackerId != m_ActorId) {
                return;
            }
            Cur++;
            OnUpdate?.Invoke(this);
            Check();
        }

        public override void Check()
        {
            if (Cur >= Count) {
                Complete();
            }
        }

        public override void GetProcess(out ObjectiveProgress process)
        {
            process.ObjectiveId = Id;
            process.Cur = Cur;
            process.Max = Count;
        }
    }
}
