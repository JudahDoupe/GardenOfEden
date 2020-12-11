using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public StateMachine<ICameraState> CameraState { get; private set; }
    public StateMachine<IUiState> UiState { get; private set; }

    public CartesianCoord FocusPoint { get; set; }

    public GameObject FocusObject;
    public PostProcessProfile PostProccessing;

    private void Start()
    {
        FocusPoint = new CartesianCoord(FocusObject.transform.position);

        CameraState = new StateMachine<ICameraState>();
        CameraState.SetState(FindObjectOfType<PlanetaryCamera>());

        UiState = new StateMachine<IUiState>();
        UiState.SetState(FindObjectOfType<CinematicUi>());
    }

    private void LateUpdate()
    {
        CameraState.State?.UpdateCamera();
        UiState.State?.UpdateUi();


        if (FocusObject != null)
        {
            FocusObject.transform.position = FocusPoint.XYZ;
        }
    }
}
