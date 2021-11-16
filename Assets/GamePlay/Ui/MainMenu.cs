using Assets.Scripts.Utils;
using Stateless;
using UnityEngine;

public class MainMenu : MenuUi
{
    private StateMachine<IState> _stateMachine = new StateMachine<IState>();
    private Transform _home;

    public override void Enable()
    {
        _home.gameObject.SetActive(true);
        _home.AnimatePosition(0.3f, new Vector3(350, 0, 0));
        Singleton.PerspectiveController.SetPerspectives(FindObjectOfType<MainMenuCamera>());
    }
    public override void Disable()
    {
        _home.AnimatePosition(0.3f, new Vector3(-350, 0, 0), () => _home.gameObject.SetActive(false));
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
        if (Input.GetKeyDown(KeyCode.Escape)) _stateMachine.SetState(this);
    }
}
