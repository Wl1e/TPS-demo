
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public enum QuestState
    {
        None,
        Failed,
        Succeeded,
        Rewarded
    }

    [System.Serializable]
    public class Quest
    {
        private int m_QuestId;
        public int QuestId => m_QuestId;
        public QuestState m_State;
        public QuestState State => m_State;
        public bool IsCompleted => State == QuestState.Succeeded;
        public bool IsReward => State == QuestState.Rewarded;

        // Objective: Optional
        private readonly Dictionary<Objective, bool> m_Objectives = new();
        public event Action<Quest> OnQuestUpdate;
        public event Action<Quest> OnQuestCompleted;

        public void Initialize(int QuestId)
        {
            m_QuestId = QuestId;
            m_State = QuestState.None;
        }

        public void SetActor(int actorId)
        {
            foreach (var obj in m_Objectives.Keys) {
                obj.SetActor(actorId);
            }
        }

        public void Destroy()
        {
            foreach (Objective objective in m_Objectives.Keys) {
                objective.Destroy();
            }
        }
        public void UpdateTask(Objective obj)
        {
            bool isComplete = true;
            foreach (var objEntry in m_Objectives) {
                if (!objEntry.Value && !objEntry.Key.IsCompleted) {
                    isComplete = false;
                    break;
                }
            }
            if (!IsCompleted && isComplete) {
                Debug.Log($"Quest {m_QuestId} Completed");
                m_State = QuestState.Succeeded;
                OnQuestCompleted?.Invoke(this);
            } else {
                OnQuestUpdate?.Invoke(this);
            }
        }

        public void AddObjective(Objective obj, bool optional = false)
        {
            m_Objectives.Add(obj, optional);
            obj.OnCompleted += UpdateTask;
            obj.OnUpdate += UpdateTask;
        }

        public void UpdateProcess(ref QuestProcess process)
        {
            var iter = m_Objectives.GetEnumerator();
            if (!iter.MoveNext()) {
                return;
            }
            process.State = m_State;
            iter.Current.Key.GetProcess(out process.Obj0);
            if (!iter.MoveNext()) {
                return;
            }
            iter.Current.Key.GetProcess(out process.Obj1);
            if (!iter.MoveNext()) {
                return;
            }
            iter.Current.Key.GetProcess(out process.Obj2);
            if (!iter.MoveNext()) {
                return;
            }
            iter.Current.Key.GetProcess(out process.Obj3);
        }

        public void FinishReward() => m_State = QuestState.Rewarded;
    }

    // 供UI显示使用
    public struct QuestProcess: IEquatable<QuestProcess>, INetworkSerializeByMemcpy
    {
        public int Id;
        public QuestState State;
        public ObjectiveProgress Obj0;
        public ObjectiveProgress Obj1;
        public ObjectiveProgress Obj2;
        public ObjectiveProgress Obj3;

        public readonly bool Equals(QuestProcess other)
        {
            return (
                Id == other.Id &&
                State == other.State &&

                Obj0.Equals(other.Obj0) &&
                Obj1.Equals(other.Obj1) &&
                Obj2.Equals(other.Obj2) &&
                Obj3.Equals(other.Obj3)
            );
        }
    }
}
