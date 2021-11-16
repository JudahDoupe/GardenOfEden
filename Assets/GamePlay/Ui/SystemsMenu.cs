using Assets.GamePlay.Cameras;
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
        Singleton.PerspectiveController.SetPerspectives(FindObjectOfType<SatelliteCamera>());
    }
    public override void Disable()
    {
        SlideToPosition(-70);
        IsActive = false;
        _stateMachine.State.Disable();
    }

    public void Globe() => _stateMachine.SetState(new ButtonState(this, "Globe", x => SetSystemsEnabled(x, new[] { SimulationType.Water }, new[] { FindObjectOfType<MainMenuCamera>() })));
    public void Land() => _stateMachine.SetState(new ButtonState(this, "Land", x => SetSystemsEnabled(x, new[] { SimulationType.Water, SimulationType.PlateTectonics }, new[] { FindObjectOfType<SatelliteCamera>() }, FindObjectOfType<PlateTectonicsToolbar>())));

    private void SetSystemsEnabled(bool enabled, SimulationType[] sims, CameraPerspective[] perspectives, MenuUi toolbar = null)
    {
        SimulationController.SetEnabledSimulations(enabled, sims);
        if (enabled) Singleton.PerspectiveController.SetPerspectives(perspectives);
            
        if (toolbar == null) return;
        
        if (enabled)
            toolbar.Enable();
        else
            toolbar.Disable();
    }
}
