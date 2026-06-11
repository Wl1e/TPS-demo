using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

namespace TPSDemo
{

    using EntityDiedEvent = Event.EntityDiedEvent;

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
            EventManager.AddListener<EntityDiedEvent>(OnObjDied);
        }

        public override void Destroy()
        {
            EventManager.RemoveListener<EntityDiedEvent>(OnObjDied);
        }

        void OnObjDied(EntityDiedEvent evt)
        {
            if (evt.Entity != Enemy) {
                return;
            }
            Cur++;
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
