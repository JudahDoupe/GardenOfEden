using System;
using System.Collections.Generic;

public static class NewPlantEventBus
{
    private static Dictionary<int, Action<Plant>> subscribers = new Dictionary<int, Action<Plant>>();
    private static int lastId = 0;

    public static void Publish(Plant plant)
    {
        foreach(var action in subscribers.Values)
        {
            action(plant);
        }
    }

    public static int Subscribe(Action<Plant> action)
    {
        lastId += 1;
        subscribers[lastId] = action;
        return lastId;
    }

    public static void Unsubscribe(int id)
    {
        subscribers.Remove(id);
    }
}