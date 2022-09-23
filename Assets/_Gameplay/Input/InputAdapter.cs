using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputAdapter
{
    /// <summary>
    /// Returns the normalized position of the mouse
    /// </summary>
    public static ButtonAction Click = new ButtonAction(_controls.Standard.Click);
    /// <summary>
    /// Returns the normalized position of the mouse
    /// </summary>
    public static ButtonAction RightClick = new ButtonAction(_controls.Standard.RightClick);
    /// <summary>
    /// Returns the normalized mopusee delta
    /// </summary>
    public static Vector2Action Drag = new Vector2Action(_controls.Standard.Drag);
    /// <summary>
    /// Returns the normalizeed movement vector
    /// </summary>
    public static Vector2Action LeftMove = new Vector2Action(_controls.Standard.LeftMove);
    /// <summary>
    /// returns the normalized rotation vector
    /// </summary>
    public static Vector2Action RightMove = new Vector2Action(_controls.Standard.RightMove);
    /// <summary>
    /// Triggers when the confim button is pressed
    /// </summary>
    public static ButtonAction Confirm = new ButtonAction(_controls.Standard.Confirm);
    /// <summary>
    /// Triggers when the cancel button is pressed
    /// </summary>
    public static ButtonAction Cancel = new ButtonAction(_controls.Standard.Cancel);


    private static Controls _controls = new Controls();
}

public class Vector2Action
{
    private readonly InputAction _input;
    private readonly List<Subscriber> _subscribers;
    private Subscriber _activeSubScriber => _subscribers.OrderBy(x => x.Priority).Last();
    private struct Subscriber
    {
        public object Id;
        public InputPriority Priority;
        public Action<Vector2> callback;
    }

    public Vector2Action(InputAction input)
    {
        _input = input;
        _subscribers = new List<Subscriber>();
        input.performed += context => Publish(context.ReadValue<Vector2>());
    }

    public void Subscribe(object subscriber, Action<Vector2> callback, InputPriority priority = InputPriority.Low) => _subscribers.Add(new Subscriber
    {
        Id = subscriber,
        Priority = priority,
        callback = callback,
    });
    public void Unubscribe(object subscriber) => _subscribers.RemoveAll(x => x.Id == subscriber);
    public Vector2 Read(object subscriber) => _activeSubScriber.Id.Equals(subscriber) ? _input.ReadValue<Vector2>() : Vector2.zero;
    private void Publish(Vector2 value) => _activeSubScriber.callback(value);
}

public class ButtonAction
{
    private readonly List<Subscriber> _subscribers;
    private Subscriber _activeSubScriber => _subscribers.OrderBy(x => x.Priority).Last();
    private struct Subscriber
    {
        public object Id;
        public InputPriority Priority;
        public Action callback;
    }

    public ButtonAction(InputAction input)
    {
        _subscribers = new List<Subscriber>();
        input.performed += context => Publish();
    }

    public void Subscribe(object subscriber, Action callback, InputPriority priority = InputPriority.Low) => _subscribers.Add(new Subscriber
    {
        Id = subscriber,
        Priority = priority,
        callback = callback,
    });
    public void Unubscribe(object subscriber) => _subscribers.RemoveAll(x => x.Id == subscriber);
    private void Publish() => _activeSubScriber.callback();
}

public enum InputPriority
{
    Low,
    Medium,
    High,
}
