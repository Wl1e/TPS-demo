
using System;
using System.Collections.Generic;

namespace TPSDemo
{
    public enum QuestState
    {
        None,
        Failed,
        Succeeded,
        Rewarded
    }

    public class Quest
    {
        private int m_QuestId;
        public int QuestId => m_QuestId;
        public bool IsCompleted { get; private set; } = false;
        public bool IsReward { get; protected set; } = false;

        // Objective: Optional
        private readonly Dictionary<Objective, bool> m_Objectives = new();
        public event Action<Quest> OnQuestUpdate;
        public event Action<Quest> OnQuestCompleted;

        public void Destroy()
        {
            foreach (Objective objective in m_Objectives.Keys) {
                objective.Destroy();
            }
        }
        public void UpdateTask(Objective obj)
        {
            if (!obj.IsCompleted) {
                return;
            }
            if (IsCompleted) {
                return;
            }

            bool isComplete = true;
            foreach (var objEntry in m_Objectives) {
                if (!objEntry.Value && !objEntry.Key.IsCompleted) {
                    isComplete = false;
                    break;
                }
            }
            if (isComplete) {
                Complete();
            } else {
                OnQuestUpdate?.Invoke(this);
            }
        }

        public void Complete()
        {
            IsCompleted = true;
            OnQuestCompleted?.Invoke(this);
        }

        public void AddObjective(Objective obj, bool optional = false)
        {
            m_Objectives.Add(obj, optional);
            if (!optional) {
                obj.OnCompleted += UpdateTask;
            }
        }

        public void UpdateProcess(ref QuestProcess process)
        {
            var iter = m_Objectives.GetEnumerator();
            if (!iter.MoveNext()) {
                return;
            }
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
    }

    // 供UI显示使用
    public struct QuestProcess: IEquatable<QuestProcess>
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
