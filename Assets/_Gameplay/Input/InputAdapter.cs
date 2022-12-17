using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputAdapter : Singleton<InputAdapter>
{
    private static Controls _controls;
    [CanBeNull] private static Settings _settings;

    /// <summary>
    ///     Returns the normalized position of the mouse
    /// </summary>
    public static PriorityAction Click { get; private set; }

    /// <summary>
    ///     Returns the normalized position of the mouse
    /// </summary>
    public static PriorityAction RightClick { get; private set; }

    /// <summary>
    ///     Returns the normalized mouse delta
    /// </summary>
    public static PriorityAction<Vector2> Drag { get; private set; }

    /// <summary>
    ///     Returns scroll delta
    /// </summary>
    public static PriorityAction<float> Scroll { get; private set; }

    /// <summary>
    ///     Returns a speed modifier
    /// </summary>
    public static PriorityAction<float> MoveModifier { get; private set; }

    /// <summary>
    ///     Returns the normalizeed movement vector
    /// </summary>
    public static PriorityAction<Vector2> LeftMove { get; private set; }

    /// <summary>
    ///     returns the normalized rotation vector
    /// </summary>
    public static PriorityAction<Vector2> RightMove { get; private set; }

    /// <summary>
    ///     Triggers when the confim button is pressed
    /// </summary>
    public static PriorityAction Confirm { get; private set; }

    /// <summary>
    ///     Triggers when the cancel button is pressed
    /// </summary>
    public static PriorityAction Cancel { get; private set; }

    /// <summary>
    ///     Triggers when the debug button is pressed
    /// </summary>
    public static PriorityAction Debug { get; private set; }

    private void Awake()
    {
        this.RunTaskInCoroutine(PlayerDataStore.GetOrCreate(), () => _settings = PlayerDataStore.GetOrCreate().Result.Settings);

        _controls = new Controls();
        _controls.Standard.Enable();
        Click = new PriorityAction(_controls.Standard.Click);
        RightClick = new PriorityAction(_controls.Standard.RightClick);
        Drag = new PriorityAction<Vector2>(_controls.Standard.Drag, x => x * (_settings?.DragSpeed ?? 1));
        Scroll = new PriorityAction<float>(_controls.Standard.Scroll, x => x * (_settings?.ScrollSpeed ?? 1));
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
    private readonly List<Subscriber> _subscribers;

    public PriorityAction(InputAction input)
    {
        _subscribers = new List<Subscriber>();
        input.started += _ => _activeSubscriber?.StartCallback?.Invoke();
        input.performed += _ => _activeSubscriber?.Callback?.Invoke();
        input.canceled += _ => _activeSubscriber?.FinishCallback?.Invoke();
    }

    [CanBeNull] private Subscriber _activeSubscriber => _subscribers.OrderBy(x => x.Priority).LastOrDefault();

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
            StartCallback = startCallback,
            Callback = callback,
            FinishCallback = finishCallback
        });
    }

    public void Unsubscribe(object subscriber)
    {
        _subscribers.RemoveAll(x => x.Id == subscriber);
        _subscribers.RemoveAll(x => x.Id == subscriber);
        _subscribers.RemoveAll(x => x.Id == subscriber);
    }

    private class Subscriber
    {
        public Action Callback;
        public Action FinishCallback;
        public object Id;
        public InputPriority Priority;
        public Action StartCallback;
    }
}

public class PriorityAction<T> where T : struct
{
    private readonly InputAction _input;
    private readonly Func<T, T> _modifier;
    private readonly List<Subscriber> _subscribers;

    public PriorityAction(InputAction input, Func<T, T> modifier = null)
    {
        _input = input;
        _modifier = modifier ?? (x => x);
        _subscribers = new List<Subscriber>();
        input.started += context => _activeSubscriber?.StartCallback?.Invoke(_modifier(context.ReadValue<T>()));
        input.performed += context => _activeSubscriber?.Callback?.Invoke(_modifier(context.ReadValue<T>()));
        input.canceled += context => _activeSubscriber?.FinishCallback?.Invoke(_modifier(context.ReadValue<T>()));
    }

    [CanBeNull] private Subscriber _activeSubscriber => _subscribers.OrderBy(x => x.Priority).LastOrDefault();

    public void Subscribe(object subscriber,
        Action<T> callback = null,
        Action<T> startCallback = null,
        Action<T> finishCallback = null,
        InputPriority priority = InputPriority.Medium)
        => _subscribers.Add(new Subscriber
        {
            Id = subscriber,
            Priority = priority,
            StartCallback = startCallback,
            Callback = callback,
            FinishCallback = finishCallback
        });

    public void Unsubscribe(object subscriber) => _subscribers.RemoveAll(x => x.Id == subscriber);

    public T Read(object subscriber)
    {
        var value = _modifier(_input.ReadValue<T>());
        return _activeSubscriber?.Id == subscriber ? value : default;
    }

    private class Subscriber
    {
        public Action<T> Callback;
        public Action<T> FinishCallback;
        public object Id;
        public InputPriority Priority;
        public Action<T> StartCallback;
    }
}

public enum InputPriority
{
    Low,
    Medium,
    High
}