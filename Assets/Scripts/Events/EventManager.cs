using System;
using System.Collections.Generic;

using Event;
public static class EventManager
{
    static readonly Dictionary<Type, Action<InternalEvent>> s_Events = new Dictionary<Type, Action<InternalEvent>>();
    static readonly Dictionary<Delegate, Action<InternalEvent>> s_EventLoopups = new Dictionary<Delegate, Action<InternalEvent>>();

    static public void AddListener<T>(Action<T> evt) where T : InternalEvent
    {
        if(s_EventLoopups.ContainsKey(evt)) {
            return;
        }
        Action<InternalEvent> newAction = gameEvent => evt((T)gameEvent);
        s_EventLoopups.Add(evt, newAction);
        if (s_Events.TryGetValue(typeof(T), out Action<InternalEvent> actions)) {
            s_Events[typeof(T)] = actions += newAction;
        } else {
            s_Events[typeof(T)] = newAction;
        }
    }
    static public void RemoveListener<T>(Action<T> evt) where T: InternalEvent
    {
        if(!s_EventLoopups.ContainsKey(evt)) {
            return;
        }
        Action<InternalEvent> savedAction = s_EventLoopups[evt];
        s_Events[typeof(T)] -= savedAction;
        if (s_Events[typeof(T)] == null) {
            s_Events.Remove(typeof(T));
        }
        s_EventLoopups.Remove(evt);
    }

    static public void Broadcast(InternalEvent gameEvent)
    {
        if(s_Events.TryGetValue(gameEvent.GetType(), out Action<InternalEvent> actions)) {
            actions.Invoke(gameEvent);
        }
    }
}
