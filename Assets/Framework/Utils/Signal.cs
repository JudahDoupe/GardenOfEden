using System;
using System.Collections.Generic;

public class Signal<T>
{
    private readonly List<Action<T>> _callbacks = new();

    public T Value { get; private set; }

    public Signal(T val) 
    {
        Value = val;
    }

    public void Subscribe(Action<T> callback) => _callbacks.Add(callback);

    public void Publish(T val)
    {
        Value = val;
        foreach (var callback in _callbacks)
            callback(Value);
    }
}