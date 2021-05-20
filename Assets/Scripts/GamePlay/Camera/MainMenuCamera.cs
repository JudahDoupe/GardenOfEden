using Assets.Scripts.Utils;
using Stateless;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    private StateMachine<UiState, UiTrigger> _stateMachine;
    private UiState _state = UiState.Closed;
    public void Open() => _stateMachine.Fire(UiTrigger.Open);
    public void Continue() => _stateMachine.Fire(UiTrigger.Continue);
    public void ResetPlanet()
    {

    }
    public void Quit()
    {
        Application.Quit();
    }

    void Start()
    {
        _stateMachine = new StateMachine<UiState, UiTrigger>(() => _state, s => _state = s);

        _stateMachine.Configure(UiState.Closed)
            .OnEntry(() =>
            {
                Singleton.CameraController.LockMovement = false;
                Singleton.CameraController.LockAltitude = false;
                Singleton.CameraController.LockRotation = false;
                Singleton.CameraController.LockCamera = false;
                Singleton.CameraController.LockFocus = false;
                transform.Find("Home").AnimateTransform(0.3f, new Vector3(-350, 0, 0), Vector3.one, false);
            })
            .Permit(UiTrigger.Open, UiState.Open);

        _stateMachine.Configure(UiState.Open)
            .OnEntry(() =>
            {
                Singleton.CameraController.LockMovement = true;
                Singleton.CameraController.LockAltitude = true;
                Singleton.CameraController.LockRotation = true;
                Singleton.CameraController.LockCamera = true;
                Singleton.CameraController.LockFocus = true;
                transform.Find("Home").gameObject.SetActive(true);
                transform.Find("Home").AnimateTransform(0.3f, new Vector3(350, 0, 0), Vector3.one);
                Camera.main.transform.AnimateTransform(1, new Vector3(0, 0, Coordinate.PlanetRadius * -4), Vector3.one);
                Camera.main.transform.AnimateRotation(1, Quaternion.identity);
                Singleton.CameraController.Focus.AnimateTransform(1, new Vector3(Coordinate.PlanetRadius * -2, 0, 0), Vector3.one);
                Singleton.CameraController.Focus.transform.AnimateRotation(1, Quaternion.identity);
            })
            .Ignore(UiTrigger.Open)
            .Permit(UiTrigger.Continue, UiState.Closed);


        Open();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Open();
        }
    }


    public enum UiState
    {
        Closed,
        Open,
    }
    public enum UiTrigger
    {
        Open,
        Continue,
    }
}
