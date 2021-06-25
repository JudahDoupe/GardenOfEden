using Stateless;
using Unity.Entities;
using UnityEngine;

public class PerspectiveController : MonoBehaviour
{
    private StateMachine<State,Trigger> _stateMachine;
    private State _state;
    private Entity _focusedEntity;

    public Transform Camera;
    public Transform Focus;


    public void ZoomIn() => _stateMachine.Fire(Trigger.ZoomIn);
    public void ZoomOut() => _stateMachine.Fire(Trigger.ZoomOut);
    public void Pause() => _stateMachine.Fire(Trigger.Pause);
    public void Unpause() => _stateMachine.Fire(Trigger.Unpause);

    public void Circle(Entity e)
    {
        _focusedEntity = e;
        _stateMachine.Fire(Trigger.Circle);
    }

    private void Start()
    {
        _state = State.MainMenu;
        _stateMachine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

        _stateMachine.Configure(State.MainMenu)
            .OnEntry(() =>
            {
                FindObjectOfType<PausedCamera>().Enable(Camera, Focus);
                FindObjectOfType<MainMenuUi>().Enable();
            })
            .OnExit(() =>
            {
                FindObjectOfType<PausedCamera>().Disable();
                FindObjectOfType<MainMenuUi>().Disable();
            })
            .Permit(Trigger.Unpause, State.Satellite);

        _stateMachine.Configure(State.Satellite)
            .OnEntry(() =>
            {
                FindObjectOfType<SatelliteCamera>().Enable(Camera, Focus);
            })
            .OnExit(() =>
            {
                FindObjectOfType<SatelliteCamera>().Disable();
            })
            .Ignore(Trigger.ZoomOut)
            .Permit(Trigger.ZoomIn, State.Landscape)
            .Permit(Trigger.Pause, State.MainMenu);

        _stateMachine.Configure(State.Landscape)
            .OnEntry(() =>
            {
                FindObjectOfType<LandscapeCamera>().Enable(Camera, Focus);
            })
            .OnExit(() =>
            {
                FindObjectOfType<LandscapeCamera>().Disable();
            })
            .Permit(Trigger.ZoomOut, State.Satellite)
            .Permit(Trigger.ZoomIn, State.Observation)
            .Permit(Trigger.Circle, State.Circle)
            .Permit(Trigger.Pause, State.MainMenu);

        _stateMachine.Configure(State.Observation)
            .OnEntry(() =>
            {
                FindObjectOfType<ObservationCamera>().Enable(Camera, Focus);
            })
            .OnExit(() =>
            {
                FindObjectOfType<ObservationCamera>().Disable();
            })
            .Permit(Trigger.ZoomOut, State.Landscape)
            .Ignore(Trigger.ZoomIn)
            .Permit(Trigger.Circle, State.Circle)
            .Permit(Trigger.Pause, State.MainMenu);

        _stateMachine.Configure(State.Circle)
            .OnEntry(() =>
            {
                FindObjectOfType<CirclingCamera>().Enable(Camera, Focus, _focusedEntity);
            })
            .OnExit(() =>
            {
                FindObjectOfType<CirclingCamera>().Disable();
            })
            .Permit(Trigger.ZoomOut, State.Observation)
            .Ignore(Trigger.ZoomIn)
            .Permit(Trigger.Pause, State.MainMenu);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))Pause();
    }


    private enum State
    {
        MainMenu,
        Satellite,
        Landscape,
        Observation,
        Circle,
    }

    private enum Trigger
    {
        ZoomIn,
        ZoomOut,
        Pause,
        Unpause,
        Circle,
    }
}
