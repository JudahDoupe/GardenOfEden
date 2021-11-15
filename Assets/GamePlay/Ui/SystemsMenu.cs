using Assets.Scripts.Utils;

public class SystemsMenu : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    
    public override void Enable()
    {
        SetAllButtonsActive(false);
        SlideToPosition(0);
        FindObjectOfType<MainMenuUi>().Disable();
        Globe();
        IsActive = true;
    }
    public override void Disable()
    {
        SlideToPosition(-70);
        IsActive = false;
    }

    public void Globe() => _stateMachine.SetState(new ButtonState(this, "Globe"));
    public void Land() => _stateMachine.SetState(new ButtonState(this, "Land",
        () => FindObjectOfType<PlateTectonicsToolbar>().Enable(),
        () => FindObjectOfType<PlateTectonicsToolbar>().Disable()));
}
