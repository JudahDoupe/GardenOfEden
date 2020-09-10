using UnityEngine;

public class CameraController : MonoBehaviour
{
    public StateMachine<ICameraState> CameraState { get; private set; }
    public StateMachine<IUiState> UiState { get; private set; }

    public Plant FocusedPlant { get; set; }
    public Vector3 FocusPoint { get; set; }

    public GameObject FocusObject;

    private void Start()
    {
        FocusPoint = Camera.main.transform.position;

        CameraState = new StateMachine<ICameraState>();
        CameraState.SetState(FindObjectOfType<ObservationCamera>());

        UiState = new StateMachine<IUiState>();
        UiState.SetState(FindObjectOfType<CinematicUi>());
    }

    private void LateUpdate()
    {
        CameraState.State?.UpdateCamera();
        UiState.State?.UpdateUi();


        if (FocusObject != null)
        {
            FocusObject.transform.position = FocusPoint;
        }
    }
}
