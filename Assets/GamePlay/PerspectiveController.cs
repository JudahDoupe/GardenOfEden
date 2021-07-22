using Stateless;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class PerspectiveController : MonoBehaviour
{
    private enum State
    {
        MainMenu,
        Satellite,
        Observation,
        Circle,
        EditDna,
    }
    private enum Trigger
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

    public void ZoomIn() => _stateMachine.Fire(Trigger.ZoomIn);
    public void ZoomOut() => _stateMachine.Fire(Trigger.ZoomOut);
    public void Pause() => _stateMachine.Fire(Trigger.Pause);
    public void Unpause() 
    {
        _isGeologyUnlocked = true; 
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
    private bool _isGeologyUnlocked;
    private bool _isBotanyUnlocked;

    private void Start()
    {
        _state = State.MainMenu;
        _stateMachine = new StateMachine<State, Trigger>(() => _state, s => _state = s);
        ConfigureMainMenu();
        ConfigureSatelite();
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
        var camera = FindObjectOfType<PausedCamera>();
        var ui = FindObjectOfType<MainMenuUi>();
        _stateMachine.Configure(State.MainMenu)
            .OnEntry(() =>
            {
                CameraUtils.TransitionState(camera.GetTargetState(new CameraState(Camera, Focus)), transitionSpeed: 2.5f, ease: Ease.In);
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
    private void ConfigureSatelite()
    {
        var camera = FindObjectOfType<SatelliteCamera>();
        var controls = FindObjectOfType<HoverAndClickControl>();
        _stateMachine.Configure(State.Satellite)
            .OnEntry(() =>
            {
                var targetState = camera.GetTargetState(new CameraState(Camera, Focus), false);
                CameraUtils.TransitionState(targetState, () =>
                {
                    camera.Enable(targetState);
                    controls.Enable();
                }, 1.5f);
                FindObjectsOfType<SpawnPlantButton>().ToList().ForEach(x => x.Open());
            })
            .OnExit(() =>
            {
                camera.Disable();
                controls.Disable();
                FindObjectsOfType<SpawnPlantButton>().ToList().ForEach(x => x.Close());
            })
            .Ignore(Trigger.ZoomOut)
            .PermitIf(Trigger.ZoomIn, State.Observation, () => _isBotanyUnlocked)
            .Permit(Trigger.Circle, State.Circle)
            .Permit(Trigger.Pause, State.MainMenu);
    }
    private void ConfigureObservation()
    {
        var camera = FindObjectOfType<ObservationCamera>();
        var controls = FindObjectOfType<PlantSelectionControl>();
        _stateMachine.Configure(State.Observation)
            .OnEntry(transition =>
            {
                var cameraState = new CameraState(Camera, Focus)
                {
                    CameraParent = Planet.Transform,
                    CameraLocalPosition = new Coordinate(Camera.position, Planet.LocalToWorld).LocalPlanet,
                    CameraLocalRotation = Quaternion.Inverse(Planet.Transform.rotation) * Camera.rotation,
                    FocusParent = Planet.Transform,
                    FocusLocalPosition = new Coordinate(CameraUtils.GetCursorWorldPosition(), Planet.LocalToWorld).LocalPlanet,
                };
                CameraUtils.SetState(cameraState);

                cameraState = camera.GetTargetState(cameraState, false);
                if (transition.Source == State.Satellite)
                {
                    var right = Planet.Transform.InverseTransformDirection(Camera.right);
                    var up = Planet.Transform.InverseTransformDirection(Camera.position.normalized);
                    var forward = Quaternion.AngleAxis(120, right) * up;

                    var cameraCoord = new Coordinate(cameraState.CameraLocalPosition);
                    cameraCoord.Lat -= camera.MaxHeight * 2;

                    cameraState.CameraLocalRotation = Quaternion.LookRotation(forward, up);
                    cameraState.CameraLocalPosition = cameraCoord.LocalPlanet;
                }
                CameraUtils.TransitionState(cameraState, () =>
                {
                    camera.Enable(cameraState);
                    controls.Enable();
                }, 1.5f);
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
    private void ConfigureCircle()
    {
        var camera = FindObjectOfType<CirclingCamera>();
        _stateMachine.Configure(State.Circle)
            .OnEntry(() =>
            {
                camera.Enable(Camera, Focus, _focusedEntity);
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
    private void ConfigureEditDna()
    {
        var camera = FindObjectOfType<CirclingCamera>();
        var ui = FindObjectOfType<DnaUi>();
        _stateMachine.Configure(State.EditDna)
            .OnEntry(() =>
            {
                camera.Enable(Camera, Focus, _focusedEntity);
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
}
