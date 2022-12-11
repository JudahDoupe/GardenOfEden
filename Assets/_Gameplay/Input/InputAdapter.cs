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
    /// Returns the normalized mouse delta
    /// </summary>
    public static PriorityAction<Vector2> Drag { get; private set; }
    /// <summary>
    /// Returns scroll delta
    /// </summary>
    public static PriorityAction<float> Scroll { get; private set; }
    /// <summary>
    /// Returns a speed modifier
    /// </summary>
    public static PriorityAction<float> MoveModifier { get; private set; }
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
    /// <summary>
    /// Triggers when the debug button is pressed
    /// </summary>
    public static PriorityAction Debug { get; private set; }

    private static Controls _controls;

    private void Awake()
    {
        _controls = new Controls();
        _controls.Standard.Enable();
        Click = new PriorityAction(_controls.Standard.Click);
        RightClick = new PriorityAction(_controls.Standard.RightClick);
        Drag = new PriorityAction<Vector2>(_controls.Standard.Drag);
        Scroll = new PriorityAction<float>(_controls.Standard.Scroll);
        MoveModifier = new PriorityAction<float>(_controls.Standard.MoveModifier);
        LeftMove = new PriorityAction<Vector2>(_controls.Standard.LeftMove);
        RightMove = new PriorityAction<Vector2>(_controls.Standard.RightMove);
        Confirm = new PriorityAction(_controls.Standard.Confirm);
        Cancel = new PriorityAction(_controls.Standard.Cancel);
        Debug = new PriorityAction(_controls.Standard.Debug);
    }
}


public class PriorityAction
{
    private class Subscriber
    {
        public object Id;
        public InputPriority Priority;
        public Action startCallback;
        public Action callback;
        public Action finishCallback;
    }
    private readonly List<Subscriber> _subscribers;
    private Subscriber _activeSubScriber => _subscribers.OrderBy(x => x.Priority).LastOrDefault();

    public PriorityAction(InputAction input)
    {
        _subscribers = new List<Subscriber>();
        input.started += _ => _activeSubScriber?.startCallback?.Invoke();
        input.performed += _ => _activeSubScriber?.callback?.Invoke();
        input.canceled += _ => _activeSubScriber?.finishCallback?.Invoke();
    }

    public void Subscribe(object subscriber,
                          Action callback = null,
                          Action startCallback = null,
                          Action finishCallback = null,
                          InputPriority priority = InputPriority.Medium)
    {
        _subscribers.Add(new Subscriber
        {
            Id = subscriber,
            Priority = priority,
            startCallback = startCallback,
            callback = callback,
            finishCallback = finishCallback,
        });
    }

    public void Unubscribe(object subscriber)
    {
        _subscribers.RemoveAll(x => x.Id == subscriber);
        _subscribers.RemoveAll(x => x.Id == subscriber);
        _subscribers.RemoveAll(x => x.Id == subscriber);
    }
}

public class PriorityAction<T> where T : struct
{
    private class Subscriber
    {
        public object Id;
        public InputPriority Priority;
        public Action<T> startCallback;
        public Action<T> callback;
        public Action<T> finishCallback;
    }
    private readonly InputAction _input;
    private readonly List<Subscriber> _subscribers;
    private Subscriber _activeSubScriber => _subscribers.OrderBy(x => x.Priority).LastOrDefault();

    public PriorityAction(InputAction input)
    {
        _input = input;
        _subscribers = new List<Subscriber>();
        input.started += context => _activeSubScriber?.startCallback?.Invoke(context.ReadValue<T>());
        input.performed += context => _activeSubScriber?.callback?.Invoke(context.ReadValue<T>());
        input.canceled += context => _activeSubScriber?.finishCallback?.Invoke(context.ReadValue<T>());
    }

    public void Subscribe(object subscriber, 
                          Action<T> callback = null, 
                          Action<T> startCallback = null, 
                          Action<T> finishCallback = null, 
                          InputPriority priority = InputPriority.Medium) => _subscribers.Add(new Subscriber
    {
        Id = subscriber,
        Priority = priority,
        startCallback = startCallback,
        callback = callback,
        finishCallback = finishCallback,
    });
    public void Unubscribe(object subscriber) => _subscribers.RemoveAll(x => x.Id == subscriber);
    public T Read(object subscriber) => _activeSubScriber?.Id == subscriber ? _input.ReadValue<T>() : default;
}

public enum InputPriority
{
    Low,
    Medium,
    High,
}
