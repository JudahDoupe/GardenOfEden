using Assets.GamePlay.Cameras;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PerspectiveController : MonoBehaviour
{
    public Transform Camera;
    public Transform Focus;
    public CameraState CurrentState => new CameraState(Camera, Focus);
    public float Altitude => Camera.position.magnitude;

    private StateMachine<CameraPerspective> _stateMachine = new StateMachine<CameraPerspective>();

    public void SetPerspective(CameraPerspective perspective, CameraTransition transition)
    {
        if (perspective == _stateMachine.State) return;

        if (_stateMachine.State != null)
        {
            _stateMachine.State.Disable();
        }

        CameraUtils.TransitionState(perspective.TransitionToState(), transition, () => {
            _stateMachine.SetState(perspective);
        });
    }

    private void Start()
    {
        SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Instant);
    }
}
