using System.Collections.Generic;

namespace TPSDemo.FSM
{
    public interface IState
    {
        public void Enter();
        public void Update();
        public void Exit();
        public void AddTransition(IState to, IPredicate predicate);
        public string GetName();
        public HashSet<Transition> Transitions { get; }
    }
}
