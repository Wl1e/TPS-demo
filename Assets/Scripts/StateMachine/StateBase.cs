using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace FSM
{
    public abstract class StateBase<StateMachineType> : IState
    {
        string m_StateName;
        protected StateMachineType m_StateMachine;

        public HashSet<Transition> Transitions { get; protected set; } = new HashSet<Transition>();

        public UnityAction OnStateEntered;
        public UnityAction OnStateExited;

        public string GetName()
        {
            return m_StateName;
        }
        public StateBase(StateMachineType stateMachine, string name)
        {
            m_StateMachine = stateMachine;
            m_StateName = name;
        }
        public void AddTransition(IState to, IPredicate predicate)
        {
            Transitions.Add(new Transition(to, predicate));
        }

        public void Enter()
        {
            foreach (var transition in Transitions) {
                transition.Predicate.Reset();
            }
            OnEnter();
            OnStateEntered?.Invoke();
        }
        public abstract void OnEnter();
        public abstract void Update();
        public void Exit()
        {
            OnExit();
            OnStateExited?.Invoke();
        }
        public abstract void OnExit();

    }

    public class Transition
    {
        public IState To;
        public IPredicate Predicate;

        public Transition(IState to, IPredicate predicate)
        {
            To = to;
            Predicate = predicate;
        }
    }


    

} // namespace FSM
