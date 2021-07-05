using System;
using System.Linq;
using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

public class ObservationCamera : MonoBehaviour
{
    [Header("Altitude")]
    public float LerpSpeed = 2f;
    public float MaxHeight = 2000;
    public float MinHeight = 2000;
    public float MaxZoomSpeed = 15f;
    public float MinZoomSpeed = 15f;
    [Header("Movement")]
    public float MaxMovementSpeed = 30f;
    public float MinMovementSpeed = 30f;
    [Header("Rotation")]
    public Vector2 RotationSpeed;
    public float VerticalAngle = 80;
    public float Fov = 60;

    public bool IsActive { get; private set; }

    private Transform _camera;
    private Transform _focus;

    private Coordinate _focusCoord;
    private float _height;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _focusCoord = new Coordinate(camera.position, Planet.LocalToWorld);
        _height = _focusCoord.Altitude - Singleton.Land.SampleHeight(_focusCoord);

        CameraUtils.SetState(new CameraState(camera, focus)
        {
            CameraParent = focus,
            CameraLocalPosition = new Vector3(0, 0, -1),
            FocusParent = Planet.Transform,
            FocusLocalPosition = _focusCoord.LocalPlanet,
        });
        CameraUtils.TransitionState(GetTargetState(camera, focus, _focusCoord), () =>
        {
            IsActive = true;
            Cursor.lockState = CursorLockMode.Locked;
        });

    }

    public void Disable()
    {
        Cursor.lockState = CursorLockMode.None;
        IsActive = false;
    }

    private void Update()
    {
        if (!IsActive) return;

        var ray = new Ray(_camera.position, _camera.forward);
        if (Physics.Raycast(ray, out var hit))
        {
            hit.transform.gameObject.SendMessage("Hover", SendMessageOptions.DontRequireReceiver);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                hit.transform.gameObject.SendMessage("Click");
            }
        }
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        var t = (MaxHeight - _height) / (MaxHeight - MinHeight);
        var translation = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * math.lerp(MaxMovementSpeed, MinMovementSpeed, t);
        var rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * RotationSpeed;
        _height = math.max(0, _height - Input.mouseScrollDelta.y * math.lerp(MaxZoomSpeed, MinZoomSpeed, t));
        _focusCoord.LocalPlanet += (_focus.localRotation * translation).ToFloat3();
        var landHeight = Singleton.Land.SampleHeight(_focusCoord);
        _focusCoord.Altitude = math.max(landHeight + MinHeight, math.lerp(_focusCoord.Altitude, _height + landHeight, Time.deltaTime * LerpSpeed));
        _focus.Rotate(0, rotation.x, 0);
        _camera.Rotate(rotation.y,0,0);

        CameraUtils.SetState(GetTargetState(_camera, _focus, _focusCoord));

        if (_height > MaxHeight)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
    }

    private CameraState GetTargetState(Transform camera, Transform focus, Coordinate focusCoord)
    {
        var xRot = camera.localRotation.eulerAngles.x;
        xRot = xRot < 180 ? math.clamp(xRot, -VerticalAngle, VerticalAngle) : math.clamp(xRot, 360 - VerticalAngle, 360 + VerticalAngle);
        var cameraRot = Quaternion.Euler(xRot, 0, 0);

        var right = Planet.Transform.InverseTransformDirection(camera.right);
        var up = Planet.Transform.InverseTransformDirection(camera.position.normalized);
        var forward = Quaternion.AngleAxis(90, right) * up;

        return new CameraState(camera, focus)
        {
            CameraParent = focus,
            CameraLocalPosition = Vector3.zero,
            CameraLocalRotation = cameraRot,
            FocusParent = Planet.Transform,
            FocusLocalPosition = focusCoord.LocalPlanet,
            FocusLocalRotation = quaternion.LookRotation(forward, up),
            FieldOfView = Fov,
        };
    }
}
