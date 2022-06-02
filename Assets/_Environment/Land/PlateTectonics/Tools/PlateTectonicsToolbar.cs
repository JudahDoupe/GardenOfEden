using UnityEngine;
using Assets.Scripts.Utils;

public class PlateTectonicsToolbar : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    private MovePlateTool _movePlateTool;
    private BreakPlateTool _breakPlateTool;
    private MergePlateTool _mergePlateTool;
    private bool _isInitialized;

    public void Initialize(PlateTectonicsData data,
        PlateTectonicsSimulation simulation,
        PlateTectonicsVisualization visualization)
    {
        _movePlateTool = FindObjectOfType<MovePlateTool>();
        _movePlateTool.Initialize(data, simulation, visualization);
        _breakPlateTool = FindObjectOfType<BreakPlateTool>();
        _breakPlateTool.Initialize(data, simulation, visualization);
        _mergePlateTool = FindObjectOfType<MergePlateTool>();
        _mergePlateTool.Initialize(data, simulation, visualization);
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
        enabled => SimulationController.SetEnabledSimulations(false, SimulationType.PlateTectonics)));

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

    private void Update()
    {
        if (IsActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (SimulationController.IsSimulationRunning(SimulationType.PlateTectonics))
                Pause();
            else
                MovePlates();
        }
    }
}