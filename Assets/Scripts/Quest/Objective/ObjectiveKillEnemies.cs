using UnityEngine;

namespace TPSDemo
{

    using ActorDiedEvent = Event.ActorDiedEvent;

    public class ObjectiveKillEnemies : Objective
    {
        public GameObject Enemy;
        public int Cur;
        public int Count;

        private int m_EnemyId = -1;

        public override void Initialize(ObjectiveConfig config)
        {
            if(config is not ObjectiveKillEnemiesConfig trueConfig) {
                return;
            }
            Enemy = trueConfig.Target;
            Count = trueConfig.Count;
            Cur = 0;
            EventManager.AddListener<ActorDiedEvent>(OnObjDied);

            if (!Enemy.TryGetComponent<EnemyController>(out var enemy)) {
                Debug.LogError("[ObjectiveKillEnemies] Enemy dont have EnemyController Component");
            } else {
                m_EnemyId = enemy.EnemyId;
            }
        }

        public override void Destroy()
        {
            EventManager.RemoveListener<ActorDiedEvent>(OnObjDied);
        }

        void OnObjDied(ActorDiedEvent evt)
        {
            var enemy = ActorManager.Instance.GetActor(evt.ActorId).GetComponent<EnemyController>();
            if (!enemy) {
                return;
            }
            Debug.Log($"Want kill {enemy.EnemyId} {m_EnemyId} {evt.AttackerId} {m_ActorId}");
            if (enemy.EnemyId != m_EnemyId || (m_ActorId != -1 && evt.AttackerId != m_ActorId)) {
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

        public override void UpdateProcess(ref ObjectiveProgress process)
        {
            if (process.ObjectiveId != Id) {
                return;
            }
            Cur = process.Cur;
            Count = process.Max;
        }
    }
}
