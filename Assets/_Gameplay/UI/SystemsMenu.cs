using Assets.GamePlay.Cameras;
using Assets.Scripts.Utils;
using UnityEngine;

public class SystemsMenu : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    
    private void Start()
    {
        SetAllButtonsActive(false);
    }
    private void Update()
    {
        if (IsActive && Input.GetKeyDown(KeyCode.Escape))
        {
            Disable();
            FindObjectOfType<MainMenu>().Enable();
        }
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
        _stateMachine.SetState(new ButtonState(this, "Globe", e =>
        {
            FindObjectOfType<SatelliteCamera>().IsDragEnabled = true;
            SimulationController.SetEnabledSimulations(e, SimulationType.Water);
            if (e) Singleton.PerspectiveController.SetPerspective(FindObjectOfType<SatelliteCamera>(), new CameraTransition { Speed = 1, Ease = EaseType.InOut });
        }));
    }
    public void Land() 
    {
        _stateMachine.SetState(new ButtonState(this, "Land", e =>
        {
            FindObjectOfType<SatelliteCamera>().IsDragEnabled = false;
            SimulationController.SetEnabledSimulations(e, SimulationType.Water, SimulationType.PlateTectonics);
            if (e) Singleton.PerspectiveController.SetPerspectives((FindObjectOfType<SatelliteCamera>(), CameraTransition.Smooth), (FindObjectOfType<LandscapeCamera>(), CameraTransition.Smooth));
            if (e) FindObjectOfType<PlateTectonicsToolbar>().Enable();
            else FindObjectOfType<PlateTectonicsToolbar>().Disable();
        }));
    }
}
