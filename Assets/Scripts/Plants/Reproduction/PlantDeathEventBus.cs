using System;
using System.Collections.Generic;

public static class PlantDeathEventBus
{
    private static readonly Dictionary<int, Action<Plant>> _subscribers = new Dictionary<int, Action<Plant>>();
    private static int _lastId = 0;

    public static void Publish(Plant plant)
    {
        foreach(var action in _subscribers.Values)
        {
            action(plant);
        }
    }

    public static int Subscribe(Action<Plant> action)
    {
        _lastId += 1;
        _subscribers[_lastId] = action;
        return _lastId;
    }

    public static void Unsubscribe(int id)
    {
        _subscribers.Remove(id);
    }
}