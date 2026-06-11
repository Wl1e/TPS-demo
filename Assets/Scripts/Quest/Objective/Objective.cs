using System;
using System.Runtime.ConstrainedExecution;
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
    public struct ObjectiveProgress
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
    }

    public abstract class Objective
    {
        public int Id = 0;
        public bool IsCompleted { get; protected set; }
        public Action<Objective> OnCompleted;
        public abstract void Check();
        // For Test
        public abstract void Initialize(ObjectiveConfig config);
        public abstract void Destroy();

        public void Complete()
        {
            IsCompleted = true;
            OnCompleted?.Invoke(this);
        }

        public abstract void GetProcess(out ObjectiveProgress process);
    }
}
