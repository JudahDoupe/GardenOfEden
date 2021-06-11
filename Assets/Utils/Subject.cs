using System;
using System.Collections.Generic;

namespace Assets.Scripts.Utils
{
    public class Subject<T>
    {
        private List<Action<T>> Subscribers = new List<Action<T>>();

        public void Publish(T item)
        {
            foreach (var subscriber in Subscribers)
            {
                subscriber(item);
            }
        }

        public void Subscribe(Action<T> callback)
        {
            Subscribers.Add(callback);
        }
    }
}
