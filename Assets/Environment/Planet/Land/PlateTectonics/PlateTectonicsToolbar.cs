using UnityEngine;
using Assets.Scripts.Utils;

public class PlateTectonicsToolbar : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    public override void Enable()
    {
        SimulationController.StartSimulations(SimulationType.PlateTectonics, SimulationType.Water);
        SetAllButtonsActive(false);
        SlideToPosition(0);
        MovePlates();
        IsActive = true;
    }
    public override void Disable()
    {
        SimulationController.StopSimulations(SimulationType.PlateTectonics, SimulationType.Water);
        SlideToPosition(70);
        _stateMachine.State = null;
        IsActive = false;
    }
    public void Pause() => _stateMachine.SetState(new ButtonState(this, "Pause", enabled => SimulationController.SetEnabledSimulations(false, SimulationType.PlateTectonics)));
    public void MovePlates() => _stateMachine.SetState(new ButtonState(this, "Move", enabled => FindObjectOfType<MovePlateTool>().IsActive = enabled));
    public void BreakPlates() => _stateMachine.SetState(new ButtonState(this, "Break", enabled => FindObjectOfType<BreakPlateTool>().IsActive = enabled));
    public void CombinePlates() => _stateMachine.SetState(new ButtonState(this, "Combine", enabled => FindObjectOfType<CombinePlateTool>().IsActive = enabled));

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
