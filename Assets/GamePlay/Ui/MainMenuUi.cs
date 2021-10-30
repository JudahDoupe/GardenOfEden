using Assets.Scripts.Utils;
using Stateless;
using UnityEngine;

public class MainMenuUi : MonoBehaviour
{
    private StateMachine<UiState, UiTrigger> _stateMachine;
    private UiState _state = UiState.Closed;

    private Transform _home;

    public void Enable() => _stateMachine.Fire(UiTrigger.Enable);
    public void Disable() => _stateMachine.Fire(UiTrigger.Disable);
    public void Continue()
    {
        FindObjectOfType<SystemsMenuUi>().Enable();
        Singleton.PerspectiveController.Unpause();
    }

    public void Quit()
    {
        Application.Quit();
    }

    void Start()
    {
        _home = transform.Find("Home");
        _stateMachine = new StateMachine<UiState, UiTrigger>(() => _state, s => _state = s);
        
        _stateMachine.Configure(UiState.Closed)
            .OnEntry(() =>
            {
                _home.AnimatePosition(0.3f, new Vector3(-350, 0, 0), () => _home.gameObject.SetActive(false));
            })
            .Ignore(UiTrigger.Disable)
            .Permit(UiTrigger.Enable, UiState.Open);

        _stateMachine.Configure(UiState.Open)
            .OnEntry(() =>
            {
                _home.gameObject.SetActive(true);
                _home.AnimatePosition(0.3f, new Vector3(350, 0, 0));
                FindObjectOfType<SystemsMenuUi>().Disable();
            })
            .Ignore(UiTrigger.Enable)
            .Permit(UiTrigger.Disable, UiState.Closed)
            .Permit(UiTrigger.Continue, UiState.Closed);


        Enable();
    }

    public enum UiState
    {
        Closed,
        Open,
    }
    public enum UiTrigger
    {
        Enable,
        Disable,
        Continue,
    }
}
