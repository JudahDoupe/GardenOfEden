/*
 * https://en.wikipedia.org/wiki/Publish%E2%80%93subscribe_pattern
 */

using System;

public interface IEventBus<T>
{
    void Publish(T eventObject);

    int Subscribe(Action<T> action);

    void Unsubscribe(int id);
}