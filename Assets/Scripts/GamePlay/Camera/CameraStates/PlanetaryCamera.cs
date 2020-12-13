using System;
using System.Linq;
using System.Timers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlanetaryCamera : MonoBehaviour, ICameraState
{
    public bool LockMovement;
    public bool LockRotation;

    public float LerpSpeed = 22f;
    public float MovementSpeed = 2f;
    public float RotationSpeed = 22f;
    public float ZoomSpeed = 1f;
    public float MinDistance = 1f;
    public float MaxDistance = 100f;

    private CameraController _controller;
    private Transform _camera;

    private Coordinate _focusTarget;
    private Coordinate _cameraTarget;

    private Vector3 Up => _controller.FocusPoint.xyz.ToVector3().normalized;
    private Vector3 Forward => (_focusTarget.xyz - new Coordinate(_cameraTarget.xyz) { Altitude = _focusTarget.Altitude }.xyz).ToVector3().normalized;

    private void Start()
    {
        _camera = Camera.main.transform;
        _controller = FindObjectOfType<CameraController>();
        _focusTarget = _controller.FocusPoint;
    }

    public void Enable()
    {
        _cameraOffset = new Vector3(0, 25, -50);
        _focusTarget = _controller.FocusPoint;
        _cameraTarget = new Coordinate(_camera.position);
    }

    public void Disable() { }

    public void UpdateCamera()
    {
        var lerpSpeed = Time.deltaTime * LerpSpeed * 2;

        UpdateFocusTarget();
        UpdateCameraTarget();

        if (Input.GetKeyDown(KeyCode.M))
        {
            var height = new Coordinate(_camera.position).Altitude - _controller.FocusPoint.Altitude;
            Singleton.LandService.PullMountain(_controller.FocusPoint, height);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Singleton.LandService.AddSpring(_controller.FocusPoint);
        }

        _controller.FocusPoint = Singleton.LandService.ClampToTerrain(Vector3.Lerp(_controller.FocusPoint.xyz, _focusTarget.xyz, lerpSpeed));
        _camera.position = Singleton.LandService.ClampAboveTerrain(Vector3.Lerp(_camera.position, _cameraTarget.xyz, lerpSpeed)).xyz;
        _camera.LookAt(_controller.FocusPoint.xyz, Up);
        _controller.PostProccessing.GetSetting<DepthOfField>().focusDistance.value = Vector3.Distance(_camera.transform.position, _controller.FocusPoint.xyz);
    }

    private Vector3 _cameraOffset;
    private void UpdateCameraTarget()
    {
        var forward = Forward;
        if (!LockRotation)
        {
            var timeFactor = Time.deltaTime * 30;

            if (Input.GetMouseButton(0))
            {
                var horizontalMovement = Input.GetAxis("Mouse X") * RotationSpeed * timeFactor;
                var invertDirectiom = Input.mousePosition.y > (Screen.height / 2) ? -1 : 1;
                forward = Quaternion.AngleAxis(horizontalMovement * invertDirectiom, Up) * forward;

                var verticalMovement = Input.GetAxis("Mouse Y") * RotationSpeed * timeFactor;
                var targetOffset = Quaternion.AngleAxis(-verticalMovement, Vector3.right) * _cameraOffset;
                if (0.05 < targetOffset.normalized.y && targetOffset.normalized.y < 0.9995)
                    _cameraOffset = targetOffset;
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                var depthMovement = 1 - Input.mouseScrollDelta.y * ZoomSpeed * timeFactor;
                _cameraOffset = (_cameraOffset * depthMovement).ClampMagnitude(MaxDistance, MinDistance);
            }
        }

        _cameraTarget = _focusTarget.xyz.ToVector3() + (Quaternion.LookRotation(forward, Up) * _cameraOffset);
    }

    private void UpdateFocusTarget()
    {
        var forward = Forward;
        var right = -Vector3.Cross(forward.normalized, Up.normalized);
        var movementMultiplier = MovementSpeed * Time.deltaTime * 30 * math.sqrt(_cameraOffset.magnitude);
        var movementVector = (right * Input.GetAxis("Horizontal") + forward * Input.GetAxis("Vertical")) * movementMultiplier;
        
        if (movementVector.magnitude > float.Epsilon && !LockMovement)
        {
            _focusTarget.xyz += movementVector.ToFloat3();
            _focusTarget = Singleton.LandService.ClampToTerrain(_focusTarget);
        }
    }
}
