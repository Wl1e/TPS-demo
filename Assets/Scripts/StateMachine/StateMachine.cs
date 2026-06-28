using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo.FSM
{
    public abstract class StateMachine : MonoBehaviour
    {
        protected Dictionary<string, IState> m_States = new Dictionary<string, IState>();
        IState m_CurrentState = null;
        IState m_PreviouState = null;
        public IState CurrentState => m_CurrentState;

        protected void FixedUpdate()
        {
            m_CurrentState?.Update();
            Transition transition = GetTransition();
            if (transition != null)
            {
                ChangeState(transition.To);
            }
        }

        public void AddState(IState state)
        {
            if (m_States.ContainsKey(state.GetName()))
            {
                Debug.LogError($"StateBase {state.GetName()} already exists in the state machine.");
                return;
            }
            m_States.Add(state.GetName(), state);
        }

        Transition GetTransition()
        {
            if (m_CurrentState != null)
            {
                foreach (Transition transition in m_CurrentState.Transitions)
                {
                    if (transition.Predicate.Evaluate())
                    {
                        return transition;
                    }
                }
            }
            return null;
        }

        public void ChangeState(string stateName)
        {
            if (!m_States.ContainsKey(stateName)) {
                return;
            }
            ChangeState(m_States[stateName]);
        }

        public void ChangeState(IState state)
        {
            m_CurrentState?.Exit();
            m_PreviouState = m_CurrentState;
            m_CurrentState = state;
            m_CurrentState.Enter();
        }

        public void Run()
        {
            m_CurrentState?.Enter();
        }

        public void Exit()
        {
            m_CurrentState?.Exit();
        }

        public abstract void InitializeFSM();
    }
}
