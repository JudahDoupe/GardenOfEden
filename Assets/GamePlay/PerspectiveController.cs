using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class PerspectiveController : MonoBehaviour
{
    [Serializable]
    public struct Transition
    {
        public State State;
        public float Speed;
        public Ease EaseIn;
    }
    public enum State
    {
        MainMenu,
        Satellite,
        Observation,
        Circle,
        EditDna,
    }
    public enum Trigger
    {
        ZoomIn,
        ZoomOut,
        Pause,
        Unpause,
        Circle,
        SelectPlant,
    }

    public Transform Camera;
    public Transform Focus;
    public List<Transition> Transitions;
    public CameraState CurrentState => new CameraState(Camera, Focus);

    public void ZoomIn() => _stateMachine.Fire(Trigger.ZoomIn);
    public void ZoomOut() => _stateMachine.Fire(Trigger.ZoomOut);
    public void Pause() => _stateMachine.Fire(Trigger.Pause);
    public void Unpause() 
    {
        _stateMachine.Fire(Trigger.Unpause); 
    }
    public void Circle(Entity e)
    {
        _isBotanyUnlocked = true;
        _focusedEntity = e;
        _stateMachine.Fire(Trigger.Circle);
    }
    public void SelectPlant(Entity e)
    {
        _focusedEntity = e;
        _stateMachine.Fire(Trigger.SelectPlant);
    }

    private StateMachine<State,Trigger> _stateMachine;
    private State _state;
    private Entity _focusedEntity;

    //TODO: Move to a perminant datastructure tied to the planet
    private bool _isBotanyUnlocked;

    private void Start()
    {
        _state = State.MainMenu;
        _stateMachine = new StateMachine<State, Trigger>(() => _state, s => _state = s);
        ConfigureMainMenu();
        ConfigureSatellite();
        ConfigureObservation();
        ConfigureCircle();
        ConfigureEditDna();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))Pause();
    }

    private void ConfigureMainMenu()
    {
        try{
            var state = State.MainMenu;
            var transition = GetTransition(state);
            var camera = FindObjectOfType<PausedCamera>();
            var ui = FindObjectOfType<MainMenuUi>();
            _stateMachine.Configure(state)
                .OnEntry(() =>
                {
                    CameraUtils.TransitionState(camera.GetTargetState(CurrentState), 
                        transitionSpeed: transition.Speed, 
                        ease: transition.EaseIn);
                    camera.Enable();
                    ui.Enable();
                })
                .OnExit(() =>
                {
                    camera.Disable();
                    ui.Disable();
                })
                .Permit(Trigger.Unpause, State.Satellite);
        }
        catch (InvalidOperationException) { }
    }
    private void ConfigureSatellite()
    {
        try
        {
            var state = State.Satellite;
            var transition = GetTransition(state);
            var camera = FindObjectOfType<SatelliteCamera>();
            var controls = FindObjectOfType<HoverAndClickControl>();
            _stateMachine.Configure(state)
                .OnEntry(() =>
                {
                    var targetState = camera.GetTargetState(CurrentState, false);
                    CameraUtils.TransitionState(targetState, () =>
                        {
                            camera.Enable(targetState);
                            FindObjectOfType<HoverAndClickControl>().Enable();
                        },
                        transitionSpeed: transition.Speed,
                        ease: transition.EaseIn);
                    FindObjectOfType<MovePlateContol>().Enable();
                    FindObjectsOfType<SpawnPlantButton>().ToList().ForEach(x => x.Open());
                })
                .OnExit(() =>
                {
                    camera.Disable();
                    FindObjectOfType<HoverAndClickControl>().Disable();
                    FindObjectOfType<MovePlateContol>().Disable();
                    FindObjectsOfType<SpawnPlantButton>().ToList().ForEach(x => x.Close());
                })
                .Ignore(Trigger.ZoomOut)
                .PermitIf(Trigger.ZoomIn, State.Observation, () => _isBotanyUnlocked)
                .Permit(Trigger.Circle, State.Circle)
                .Permit(Trigger.Pause, State.MainMenu);
        }
        catch (InvalidOperationException) { }
    }
    private void ConfigureObservation()
    {
        try
        {
            var state = State.Observation;
            var transition = GetTransition(state);
            var camera = FindObjectOfType<ObservationCamera>();
            var controls = FindObjectOfType<PlantSelectionControl>();
            _stateMachine.Configure(state)
                .OnEntry(from =>
                {
                    if (from.Source == State.Circle)
                    {
                        camera.Enable(CurrentState);
                        controls.Enable();
                    }
                    else
                    {
                        var cameraState = camera.GetTargetState(CurrentState, false);
                        CameraUtils.TransitionState(cameraState, () =>
                            {
                                camera.Enable(cameraState);
                                controls.Enable();
                            },
                            transitionSpeed: transition.Speed,
                            ease: transition.EaseIn);
                    }
                })
                .OnExit(() =>
                {
                    camera.Disable();
                    controls.Disable();
                })
                .Permit(Trigger.ZoomOut, State.Satellite)
                .Ignore(Trigger.ZoomIn)
                .Permit(Trigger.Circle, State.Circle)
                .Permit(Trigger.SelectPlant, State.EditDna)
                .Permit(Trigger.Pause, State.MainMenu);
        }
        catch (InvalidOperationException) { }
    }
    private void ConfigureCircle()
    {
        try
        {
            var state = State.Circle;
            var transition = GetTransition(state);
            var camera = FindObjectOfType<CirclingCamera>();
            _stateMachine.Configure(state)
                .OnEntry(() =>
                {
                    var targetState = camera.GetTargetState(CurrentState, _focusedEntity);
                    CameraUtils.TransitionState(targetState, () =>
                        {
                            camera.Enable(CurrentState, _focusedEntity);
                        },
                        transitionSpeed: transition.Speed,
                        ease: transition.EaseIn);
                    FindObjectsOfType<SpawnPlantButton>().ToList().ForEach(x => x.Open());
                })
                .OnExit(() =>
                {
                    camera.Disable();
                })
                .Permit(Trigger.ZoomOut, State.Observation)
                .Ignore(Trigger.ZoomIn)
                .Ignore(Trigger.Circle)
                .Permit(Trigger.Pause, State.MainMenu);
        }
        catch (InvalidOperationException) { }
    }
    private void ConfigureEditDna()
    {
        try
        {
            var state = State.EditDna;
            var transition = GetTransition(state);
            var camera = FindObjectOfType<CirclingCamera>();
            var ui = FindObjectOfType<DnaUi>();
            _stateMachine.Configure(state)
                .OnEntry(() =>
                {
                    var targetState = camera.GetTargetState(CurrentState, _focusedEntity);
                    CameraUtils.TransitionState(targetState, () =>
                        {
                            camera.Enable(CurrentState, _focusedEntity);
                        },
                        transitionSpeed: transition.Speed,
                        ease: transition.EaseIn);
                    ui.EditDna(_focusedEntity);
                })
                .OnExit(() =>
                {
                    camera.Disable();
                    ui.Done();
                })
                .Permit(Trigger.Circle, State.Circle)
                .Ignore(Trigger.ZoomOut)
                .Ignore(Trigger.Pause);
        }
        catch (InvalidOperationException) { }
    }


    private Transition GetTransition(State state) => Transitions.Any(x => x.State == state)
        ? Transitions.First(x => x.State == state)
        : new Transition { EaseIn = Ease.InOut, Speed = 1f };
}
