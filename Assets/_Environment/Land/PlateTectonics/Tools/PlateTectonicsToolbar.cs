using Assets.GamePlay.Cameras;
using UnityEngine;
using Assets.Scripts.Utils;

public class PlateTectonicsToolbar : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    private MovePlateTool _movePlateTool;
    private BreakPlateTool _breakPlateTool;
    private MergePlateTool _mergePlateTool;
    private PlateTectonicsSimulation _simulation;
    private PlateTectonicsVisualization _visualization;
    private PlateBaker _baker;
    private bool _isInitialized;

    public void Initialize(PlateTectonicsData data)
    {
        _simulation = FindObjectOfType<PlateTectonicsSimulation>();
        _visualization = FindObjectOfType<PlateTectonicsVisualization>();
        _baker = FindObjectOfType<PlateBaker>();
        _movePlateTool = FindObjectOfType<MovePlateTool>();
        _movePlateTool.Initialize(data, _simulation, _visualization, _baker);
        _breakPlateTool = FindObjectOfType<BreakPlateTool>();
        _breakPlateTool.Initialize(data, _simulation, _visualization, _baker);
        _mergePlateTool = FindObjectOfType<MergePlateTool>();
        _mergePlateTool.Initialize(data, _simulation, _visualization);
        _isInitialized = true;
    }
    public override void Enable()
    {
        if (!_isInitialized)
            return;
        SetAllButtonsActive(false);
        SlideToPosition(0);
        MovePlates();
        IsActive = true;
    }
    public override void Disable()
    {
        SlideToPosition(70);
        _stateMachine.State.Disable();
        IsActive = false;
    }

    public void Pause() => _stateMachine.SetState(new ButtonState(this, "Pause",
        enabled => 
        {
            if (enabled)
                _simulation.Enable();
            else
                _simulation.Disable();
        }));
    
    public void Camera() => _stateMachine.SetState(new ButtonState(this, "Camera",
        enabled => 
        {
            if (enabled)
                CameraController.SetPerspective(FindObjectOfType<LandscapeCamera>(), CameraTransition.SmoothFast);
            else
                CameraController.SetPerspective(FindObjectOfType<SatelliteCamera>(), CameraTransition.Smooth);
        }));

    public void MovePlates() => _stateMachine.SetState(new ButtonState(this, "Move",
        enabled =>
        {
            if (enabled)
                _movePlateTool.Enable();
            else
                _movePlateTool.Disable();
        }));

    public void BreakPlates() => _stateMachine.SetState(new ButtonState(this, "Break",
        enabled =>
        {
            if (enabled)
                _breakPlateTool.Enable();
            else
                _breakPlateTool.Disable();
        }));

    public void CombinePlates() => _stateMachine.SetState(new ButtonState(this, "Combine",
        enabled =>
        {
            if (enabled)
                _mergePlateTool.Enable();
            else
                _mergePlateTool.Disable();
        }));
}