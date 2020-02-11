using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum State
    {
        Cinematic,
        Birdseye,
        Inspection,
    }

    public void SetState(State state)
    {
        _currentState = _states[state];
        _currentState.Transition();
    }

    /* INNER MECHINATIONS */

    private Dictionary<State, ICameraState> _states;
    private ICameraState _currentState;

    private void Start()
    {
        _states = new Dictionary<State, ICameraState>
        {
            {State.Cinematic, new Cinematic()},
            {State.Birdseye, new BirdsEye()},
            {State.Inspection, new Inspection()},
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
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            SetState(State.Inspection);
        }
    }

    private void LateUpdate()
    {
        _currentState.Update();
    }
}
