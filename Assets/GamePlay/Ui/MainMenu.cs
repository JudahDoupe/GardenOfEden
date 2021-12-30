using Assets.GamePlay.Cameras;
using Assets.Scripts.Utils;
using Stateless;
using UnityEngine;

public class MainMenu : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    private Transform _home;
    private Controls _controls;

    public override void Enable()
    {
        _controls = new Controls();
        _controls.UI.Enable();
        _home.gameObject.SetActive(true);
        _home.AnimatePosition(0.3f, new Vector3(350, 0, 0));
        Singleton.PerspectiveController.SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Smooth);
        SimulationController.StartSimulations(SimulationType.Water);
    }
    public override void Disable()
    {
        _controls.UI.Disable();
        _controls.Dispose();
        _home.AnimatePosition(0.3f, new Vector3(-350, 0, 0), () => _home.gameObject.SetActive(false));
        SimulationController.StopSimulations(SimulationType.Water);
    }
    public void Continue()
    {
        _stateMachine.SetState(FindObjectOfType<SystemsMenu>());
    }
    public void Quit()
    {
        Application.Quit();
    }

    void Start()
    {
        _home = transform.Find("Home");
        _stateMachine.SetState(this);
    }
    void Update()
    {
        _controls.UI.Cancel.performed += _stateMachine.SetState(this);
    }
}
