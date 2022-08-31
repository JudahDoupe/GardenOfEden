using Assets.GamePlay.Cameras;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    public Camera Camera;
    public Transform Focus;
    
    private static StateMachine<CameraPerspective> _stateMachine;
    private void Start()
    {
        _stateMachine = new StateMachine<CameraPerspective>();

        SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Instant);
    }
    
    public static float Altitude => Instance.Camera.transform.position.magnitude;
    public static CameraState CurrentState => new CameraState(Instance.Camera, Instance.Focus);
    public static void SetPerspective(CameraPerspective perspective, CameraTransition transition)
    {
        if (perspective == _stateMachine.State) return;

        if (_stateMachine.State != null)
        {
            _stateMachine.State.Disable();
        }

        CameraUtils.TransitionState(perspective.TransitionToState(), transition, () => {
            _stateMachine.SetState(perspective);
        });
    }
}