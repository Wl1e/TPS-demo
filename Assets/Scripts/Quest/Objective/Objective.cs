using System;
using System.Runtime.ConstrainedExecution;
using Unity.Netcode;
using UnityEngine;


namespace TPSDemo
{

    [Serializable]
    public struct PairEntry<KeyType, ValueType>
    {
        public KeyType Key;
        public ValueType Value;
        public PairEntry(KeyType key, ValueType value)
        {
            Key = key;
            Value = value;
        }
    }


    public abstract class ObjectiveConfig: ScriptableObject
    {
        public int Id;
        public abstract string GetObjectiveText();
    };

    [Serializable]
    public struct ObjectiveProgress: INetworkSerializeByMemcpy
    {
        public int ObjectiveId;
        public int Cur;
        public int Max;

        public readonly bool Equals(ObjectiveProgress other)
        {
            return (
                ObjectiveId == other.ObjectiveId &&
                Cur == other.Cur &&
                Max == other.Max
            );
        }

        public ObjectiveProgress(int _ = 0)
        {
            ObjectiveId = 0;
            Cur = 0;
            Max = 0;
        }

        public readonly bool IsEmpty => ObjectiveId == 0;
    }


    public abstract class Objective
    {
        public int Id = 0;
        protected int m_ActorId = -1;
        public bool IsCompleted { get; protected set; }
        public Action<Objective> OnCompleted;
        public Action<Objective> OnUpdate;
        public abstract void Check();
        // For Test
        public abstract void Initialize(ObjectiveConfig config);
        public abstract void Destroy();

        public void Complete()
        {
            IsCompleted = true;
            OnCompleted?.Invoke(this);
            Debug.Log($"Objective: {Id} Completed");
        }

        public void SetActor(int actorId)
        {
            m_ActorId = actorId;
            if (!ActorManager.Instance.GetActor(m_ActorId).TryGetComponent<PlayerController>(out var player)) {
                Debug.LogError("not player");
                return;
            }
        }

        public abstract void GetProcess(out ObjectiveProgress process);
    }
}
