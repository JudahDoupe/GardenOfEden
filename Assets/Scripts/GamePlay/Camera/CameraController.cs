using UnityEngine;

public class CameraController : MonoBehaviour
{
    public StateMachine<ICameraState> CameraState { get; private set; }
    public StateMachine<IUiState> UiState { get; private set; }

    public Plant FocusedPlant { get; set; }

    private void Start()
    {
        CameraState = new StateMachine<ICameraState>();
        UiState = new StateMachine<IUiState>();

        CameraState.SetState(new ObservationCameraState(this));
        UiState.SetState(FindObjectOfType<CinematicUi>());
    }

    private void LateUpdate()
    {
        CameraState.State?.UpdateCamera();
    }
}
