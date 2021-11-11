using UnityEngine;
using Assets.Scripts.Utils;

public class PlateTectonicsToolbar : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    public void Enable()
    {
        SetAllButtonsActive(false);
        SlideToPosition(0);
        MovePlates();
        IsActive = true;
    }
    public void Disable()
    {
        SlideToPosition(70);
        _stateMachine.State = null;
        IsActive = false;
    }
    public void Pause() => _stateMachine.SetState(new PauseButton(this, SimulationType.PlateTectonics, "Pause"));
    public void MovePlates() => _stateMachine.SetState(new ToolButton(this, FindObjectOfType<MovePlateTool>(), "Move"));
    public void BreakPlates() => _stateMachine.SetState(new ToolButton(this, FindObjectOfType<BreakPlateTool>(), "Break"));
    public void CombinePlates() => _stateMachine.SetState(new ToolButton(this, FindObjectOfType<CombinePlateTool>(), "Combine"));

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
