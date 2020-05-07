using CameraState;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public float MoveSpeed = 0.25f;
    public float LookSpeed = 0.75f;
    public Vector3 TargetPosition;
    public Vector3 TargetFocusPosition;

    public Focus PrimaryFocus { get; private set; }
    public Focus SecondaryFocus { get; private set; }
    public CameraStateMachine State { get; private set; }

    public PostProcessProfile PPProfile;

    private void Start()
    {
        PrimaryFocus = new Focus();
        SecondaryFocus = new Focus();
        State = new CameraStateMachine();
        PrimaryFocus.Object = FindObjectOfType<Plant>().transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            State.Set(CameraStateType.Cinematic);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            State.Set(CameraStateType.Birdseye);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            State.Set(CameraStateType.Species);
        }
    }

    private void LateUpdate()
    {
        State.Update();
        LerpTowardTargets();
        UpdateDepthOfField();
    }

    private void LerpTowardTargets()
    {
        var targetPosition = Vector3.Lerp(transform.position, TargetPosition, MoveSpeed * Time.deltaTime);
        targetPosition.y = Mathf.Max(targetPosition.y, DI.LandService.SampleTerrainHeight(targetPosition) + 0.5f);
        transform.position = targetPosition;

        var targetRotation = Quaternion.LookRotation(TargetFocusPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, LookSpeed * Time.deltaTime);
    }

    private void UpdateDepthOfField()
    {
        var dof = PPProfile.GetSetting<DepthOfField>();
        var focalLength = PrimaryFocus.Object == null ? 10 : Vector3.Distance(Camera.main.transform.position, PrimaryFocus.GetPosition());
        dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, focalLength, Time.deltaTime);
    }
}
