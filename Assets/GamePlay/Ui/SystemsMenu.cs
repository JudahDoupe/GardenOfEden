using Assets.GamePlay.Cameras;
using Assets.Scripts.Utils;

public class SystemsMenu : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    
    private void Start()
    {
        SetAllButtonsActive(false);
    }
    public override void Enable()
    {
        SlideToPosition(0);
        IsActive = true;
        Globe();
    }
    public override void Disable()
    {
        SlideToPosition(-70);
        IsActive = false;
        _stateMachine.State.Disable();
    }

    public void Globe()
    {
        var systems = new[] { SimulationType.Water };
        var perspective = FindObjectOfType<SatelliteCamera>();
        var transition = new CameraTransition { Speed = 1, Ease = Ease.InOut };
        _stateMachine.SetState(new ButtonState(this, "Globe", e => SetSystemsEnabled(e, systems, perspective, transition)));
    }
    public void Land() 
    {
        var systems = new[] { SimulationType.Water, SimulationType.PlateTectonics };
        var perspective = FindObjectOfType<SatelliteCamera>();
        var toolbar = FindObjectOfType<PlateTectonicsToolbar>();
        var transition = new CameraTransition { Speed = 1, Ease = Ease.InOut };
        _stateMachine.SetState(new ButtonState(this, "Land", e => SetSystemsEnabled(e, systems, perspective, transition, toolbar)));
    }
    private void SetSystemsEnabled(bool enabled, SimulationType[] sims, CameraPerspective perspective, CameraTransition transition, MenuUi toolbar = null)
    {
        SimulationController.SetEnabledSimulations(enabled, sims);
        if (enabled) Singleton.PerspectiveController.SetPerspective(perspective, transition);
        if (toolbar == null) return;
        if (enabled) toolbar.Enable();
        else toolbar.Disable();
    }
}
