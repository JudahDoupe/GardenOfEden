using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum State
    {
        Cinematic,
        Birdseye
    }

    public void SetState(State state)
    {
        _currentState = _states[state];
        _currentState.Transition();
    }

    /* INNER MECHINATIONS */

    private CameraTransform _transform;
    private CameraFocus _focus;

    private Dictionary<State, CameraState> _states;
    private CameraState _currentState;

    private void Start()
    {
        _focus = FindObjectOfType<CameraFocus>();
        _transform = FindObjectOfType<CameraTransform>();
        _states = new Dictionary<State, CameraState>
        {
            {State.Cinematic, new Cinematic(_transform, _focus)},
            {State.Birdseye, new BirdsEye(_transform, _focus)},
        };

        SetState(State.Cinematic);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetState(State.Cinematic);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            SetState(State.Birdseye);
        }
    }

    private void LateUpdate()
    {
        _currentState.Update();
    }
}
