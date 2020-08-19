using System;
using System.Collections.Generic;

public class MessageBus<T>
{
    private readonly Dictionary<int, Action<T>> _subscribers = new Dictionary<int, Action<T>>();
    private int _lastId = 0;

    public void Publish(T obj)
    {
        foreach (var action in _subscribers.Values)
        {
            action(obj);
        }
    }

    public int Subscribe(Action<T> action)
    {
        _lastId += 1;
        _subscribers[_lastId] = action;
        return _lastId;
    }

    public void Unsubscribe(int id)
    {
        _subscribers.Remove(id);
    }
}
