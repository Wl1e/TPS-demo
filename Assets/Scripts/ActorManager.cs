using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    public class ActorManager : Singleton<ActorManager>
    {
        Dictionary<int, Actor> m_Actors = new Dictionary<int, Actor>();
        public Dictionary<int, Actor> Actors => m_Actors;
        public void AddActor(Actor actor)
        {
            m_Actors.Add(actor.Id, actor);
        }

        public void RemoveActor(Actor actor)
        {
            m_Actors.Remove(actor.Id);
        }

        public Actor GetActor(int actorId)
        {
            return m_Actors.GetValueOrDefault(actorId, null);
        }
    }
}
