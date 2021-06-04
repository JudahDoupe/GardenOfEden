using Stateless;
using UnityEngine;

public class PerspectiveController : MonoBehaviour
{
    private StateMachine<State,Trigger> _stateMachine;
    private State _state;
    private State _prevState = State.MainMenu;
    private StateMachine<State, Trigger>.TriggerWithParameters<State> _unpause;

    public Transform Camera;
    public Transform Focus;


    public void ZoomIn() => _stateMachine.Fire(Trigger.ZoomIn);
    public void ZoomOut() => _stateMachine.Fire(Trigger.ZoomOut);
    public void Pause()
    {
        _prevState = _state;
        _stateMachine.Fire(Trigger.Pause);
    }

    public void Unpause() => _stateMachine.Fire(_unpause, _prevState);


    private void Start()
    {
        _state = State.MainMenu;
        _stateMachine = new StateMachine<State, Trigger>(() => _state, s => _state = s);
        _unpause = _stateMachine.SetTriggerParameters<State>(Trigger.Unpause);

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
            .PermitDynamic(_unpause, state => state == State.MainMenu ? State.Satellite : state);

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
    }

    private enum Trigger
    {
        ZoomIn,
        ZoomOut,
        Pause,
        Unpause,
    }
}
