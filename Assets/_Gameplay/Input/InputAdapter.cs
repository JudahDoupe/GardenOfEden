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
    public static InputAction<Vector2> Click;
    /// <summary>
    /// Returns the normalized mopusee delta
    /// </summary>
    public static InputAction<Vector2> Drag;
    /// <summary>
    /// Returns the normalizeed movement vector
    /// </summary>
    public static InputAction<Vector2> LeftMove;
    /// <summary>
    /// returns the normalized rotation vector
    /// </summary>
    public static InputAction<Vector2> RightMove;
    /// <summary>
    /// Triggers when the confim button is pressed
    /// </summary>
    public static InputAction<bool> Confirm;
    /// <summary>
    /// Triggers when the cancel button is pressed
    /// </summary>
    public static InputAction<bool> Cancel;

    private static Controls _controls;

    public void Start()
    {
        _controls = new Controls();
        Click = new InputAction<Vector2>(_controls.Standard.Click, () => Mouse.current.position.ReadValue());
        Drag = new InputAction<Vector2>(_controls.Standard.Drag);
        LeftMove = new InputAction<Vector2>(_controls.Standard.LeftMove);
        RightMove = new InputAction<Vector2>(_controls.Standard.RightMove);
        Confirm = new InputAction<bool>(_controls.Standard.Confirm);
        Cancel = new InputAction<bool>(_controls.Standard.Cancel);
    }
}

public enum InputPriority
{
    Low,
    Medium,
    High,
}

public class InputAction<T> where T : struct
{
    private readonly InputAction _input;
    private readonly List<Subscriber<T>> _subscribers;
    private Subscriber<T> _activeSubScriber => _subscribers.OrderBy(x => x.Priority).Last();
    private struct Subscriber<T2>
    {
        public object Id;
        public InputPriority Priority;
        public Action<T2> callback;
    }

    public InputAction(InputAction input)
    {
        _input = input;
        _subscribers = new List<Subscriber<T>>();
        input.performed += context => Publish(context.ReadValue<T>());
    }

    public InputAction(InputAction input, Func<T> output)
    {
        _input = input;
        _subscribers = new List<Subscriber<T>>();
        input.performed += context => Publish(output());
    }

    public void Subscribe(object subscriber, Action<T> callback, InputPriority priority = InputPriority.Low) => _subscribers.Add(new Subscriber<T>
    {
        Id = subscriber,
        Priority = priority,
        callback = callback,
    });
    public void Unubscribe(object subscriber) => _subscribers.RemoveAll(x => x.Id == subscriber);
    public T Read(object subscriber) => _activeSubScriber.Id.Equals(subscriber) ? _input.ReadValue<T>() : default;
    private void Publish(T value) => _activeSubScriber.callback(value);
}
