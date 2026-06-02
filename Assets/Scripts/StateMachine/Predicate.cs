
using System;
using UnityEngine;

namespace FSM
{
    public interface IPredicate
    {
        public bool Evaluate();
        public void Reset();
    }

    public class FuncPredicate : IPredicate
    {
        private Func<bool> m_Predicate;
        public FuncPredicate(Func<bool> predicate)
        {
            m_Predicate = predicate;
        }
        public bool Evaluate()
        {
            return m_Predicate.Invoke();
        }
        public void Reset() { }
    }

    public class TimeoutPredicate : IPredicate
    {
        float m_t = 0f;
        float m_Duration = 0f;
        public TimeoutPredicate(float duration)
        {
            m_Duration = duration;
        }
        public bool Evaluate()
        {
            m_t += Time.deltaTime;
            if (m_t >= m_Duration) {
                return true;
            }
            return false;
        }
        public void Reset()
        {
            m_t = 0f;
        }
    }
}
