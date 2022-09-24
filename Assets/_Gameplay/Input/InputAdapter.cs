using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputAdapter : Singleton<InputAdapter>
{
    /// <summary>
    /// Returns the normalized position of the mouse
    /// </summary>
    public static PriorityAction Click { get; private set; }
    /// <summary>
    /// Returns the normalized position of the mouse
    /// </summary>
    public static PriorityAction RightClick { get; private set; }
    /// <summary>
    /// Returns the normalized mopusee delta
    /// </summary>
    public static PriorityAction<Vector2> Drag { get; private set; }
    /// <summary>
    /// Returns the normalizeed movement vector
    /// </summary>
    public static PriorityAction<Vector2> LeftMove { get; private set; }
    /// <summary>
    /// returns the normalized rotation vector
    /// </summary>
    public static PriorityAction<Vector2> RightMove { get; private set; }
    /// <summary>
    /// Triggers when the confim button is pressed
    /// </summary>
    public static PriorityAction Confirm { get; private set; }
    /// <summary>
    /// Triggers when the cancel button is pressed
    /// </summary>
    public static PriorityAction Cancel { get; private set; }

    private static Controls _controls;

    private void Start()
    {
        _controls = new Controls();
        _controls.Standard.Enable();
        Click = new PriorityAction(_controls.Standard.Click);
        RightClick = new PriorityAction(_controls.Standard.RightClick);
        Drag = new PriorityAction<Vector2>(_controls.Standard.Drag);
        LeftMove = new PriorityAction<Vector2>(_controls.Standard.LeftMove);
        RightMove = new PriorityAction<Vector2>(_controls.Standard.RightMove);
        Confirm = new PriorityAction(_controls.Standard.Confirm);
        Cancel = new PriorityAction(_controls.Standard.Cancel);
    }
}


public class PriorityAction
{
    private class Subscriber
    {
        public object Id;
        public InputPriority Priority;
        public Action callback;
    }
    private readonly List<Subscriber> _subscribers;
    private Subscriber _activeSubScriber => _subscribers.OrderBy(x => x.Priority).LastOrDefault();

    public PriorityAction(InputAction input)
    {
        _subscribers = new List<Subscriber>();
        input.performed += Publish;
    }

    public void Subscribe(object subscriber, Action callback, InputPriority priority = InputPriority.Medium) => _subscribers.Add(new Subscriber
    {
        Id = subscriber,
        Priority = priority,
        callback = callback,
    });
    public void Unubscribe(object subscriber) => _subscribers.RemoveAll(x => x.Id == subscriber);
    public void Publish(InputAction.CallbackContext context) => _activeSubScriber?.callback();
}

public class PriorityAction<T> where T : struct
{
    private class Subscriber
    {
        public object Id;
        public InputPriority Priority;
        public Action<T> callback;
    }
    private readonly InputAction _input;
    private readonly List<Subscriber> _subscribers;
    private Subscriber _activeSubScriber => _subscribers.OrderBy(x => x.Priority).LastOrDefault();

    public PriorityAction(InputAction input)
    {
        _input = input;
        _subscribers = new List<Subscriber>();
        input.performed += Publish;
    }

    public void Subscribe(object subscriber, Action<T> callback, InputPriority priority = InputPriority.Medium) => _subscribers.Add(new Subscriber
    {
        Id = subscriber,
        Priority = priority,
        callback = callback,
    });
    public void Unubscribe(object subscriber) => _subscribers.RemoveAll(x => x.Id == subscriber);
    public T Read(object subscriber) => _activeSubScriber?.Id == subscriber ? _input.ReadValue<T>() : default;
    public void Publish(InputAction.CallbackContext context) => _activeSubScriber?.callback(context.ReadValue<T>());
}

public enum InputPriority
{
    Low,
    Medium,
    High,
}
