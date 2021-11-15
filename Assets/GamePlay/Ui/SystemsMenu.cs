using Assets.Scripts.Utils;

public class SystemsMenu : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    
    private void Start()
    {
        SetAllButtonsActive(false);
        Globe();
    }
    public override void Enable()
    {
        SlideToPosition(0);
        IsActive = true;
        _stateMachine.State.Enable();
    }
    public override void Disable()
    {
        SlideToPosition(-70);
        IsActive = false;
        _stateMachine.State.Disable();
    }

    public void Globe() => _stateMachine.SetState(new ButtonState(this, "Globe", x => SetSystemsEnabled(x, null, SimulationType.Water)));
    public void Land() => _stateMachine.SetState(new ButtonState(this, "Land", x => SetSystemsEnabled(x, FindObjectOfType<PlateTectonicsToolbar>(), SimulationType.Water, SimulationType.PlateTectonics)));

    private void SetSystemsEnabled(bool enabled, MenuUi toolbar, params SimulationType[] sims)
    {
        SimulationController.SetEnabledSimulations(enabled, sims);
        if (toolbar != null)
        {
            if (enabled)
                toolbar.Enable();
            else
                toolbar.Disable();
        }
    }
}
