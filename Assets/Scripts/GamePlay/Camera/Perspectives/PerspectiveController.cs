using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using Stateless;
using Unity.Mathematics;
using UnityEngine;

public class PerspectiveController : MonoBehaviour
{
    private StateMachine<State,Trigger> _stateMachine;
    private State _state;

    public Transform Camera;
    public Transform Focus;


    private void Start()
    {
        _state = State.MainMenu;
        _stateMachine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

        _stateMachine.Configure(State.MainMenu)
            .OnEntry(() =>
            {
                Camera.parent = null;
                var targetPos = new Vector3(-750, 0, -2400);
                var time = math.sqrt(Vector3.Distance(targetPos, Camera.position)) / 25f;
                Camera.AnimatePosition(time, targetPos);
                Camera.AnimateRotation(time, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            })
            .OnExit(() =>
            {

            })
            .Permit(Trigger.ZoomIn, State.Satellite)
            .Permit(Trigger.Continue, State.Satellite);

        _stateMachine.Configure(State.Satellite)
            .OnEntry(() =>
            {
                Focus.position = Vector3.zero;
                Focus.rotation = Quaternion.LookRotation(Vector3.Normalize(Focus.position - Camera.position), Vector3.up);
                Camera.parent = Focus;
                var targetPos = new Vector3(0, 0, -2000);
                var time = math.sqrt(Vector3.Distance(targetPos, Camera.position)) / 25f;
                Camera.AnimatePosition(time, targetPos);
                Camera.AnimateRotation(time, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            })
            .OnExit(() =>
            {

            })
            .Permit(Trigger.ZoomIn, State.Satellite)
            .Permit(Trigger.Continue, State.Satellite);
    }


    private enum State
    {
        MainMenu,
        Satellite,
    }

    private enum Trigger
    {
        ZoomIn,
        ZoomOut,
        Pause,
        Continue,
    }
}
