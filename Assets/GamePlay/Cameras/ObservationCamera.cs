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

    private Coordinate _cameraCoord;
    private float _height;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _cameraCoord = new Coordinate(camera.position, Planet.LocalToWorld);
        _height = _cameraCoord.Altitude - Singleton.Land.SampleHeight(_cameraCoord);

        CameraUtils.SetState(new CameraState(camera, focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = _cameraCoord.LocalPlanet,
            CameraLocalRotation = Quaternion.Inverse(Planet.Transform.rotation) * _camera.rotation,
            FocusParent = Planet.Transform,
            FocusLocalPosition = new Coordinate(CameraUtils.GetCursorWorldPosition(), Planet.LocalToWorld).LocalPlanet,
        });
        CameraUtils.TransitionState(GetTargetState(camera, focus), () =>
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

        CameraUtils.SetState(GetTargetState(_camera, _focus));

        if (_height > MaxHeight)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
    }

    private CameraState GetTargetState(Transform camera, Transform focus)
    {
        var t = (MaxHeight - _height) / (MaxHeight - MinHeight);
        var translation = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * math.lerp(MaxMovementSpeed, MinMovementSpeed, t);
        var rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * RotationSpeed;

        var right = camera.right;
        var up = camera.position.normalized;
        var forward = Quaternion.AngleAxis(90, right) * up;

        var cameraRotation = Quaternion.LookRotation(camera.forward, up);
        cameraRotation = Quaternion.AngleAxis(rotation.x, up) * cameraRotation;
        right = camera.right;
        var desiredVerticalRotation = Quaternion.AngleAxis(rotation.y, right) * cameraRotation;
        var desiredAngle = Quaternion.Angle(quaternion.LookRotation(forward, up), desiredVerticalRotation);
        var currentAngle = Quaternion.Angle(quaternion.LookRotation(forward, up), cameraRotation);
        if (desiredAngle <= VerticalAngle || desiredAngle < currentAngle)
        {
            cameraRotation = desiredVerticalRotation;
        }
        var localCameraRotation = Quaternion.Inverse(Planet.Transform.rotation) * cameraRotation;
        var localFocusRotation = Quaternion.Inverse(Planet.Transform.rotation) * quaternion.LookRotation(forward, up);

        _height = math.max(0, _height - Input.mouseScrollDelta.y * math.lerp(MaxZoomSpeed, MinZoomSpeed, t));
        _cameraCoord.LocalPlanet += (localFocusRotation * translation).ToFloat3();
        var landHeight = Singleton.Land.SampleHeight(_cameraCoord);
        _cameraCoord.Altitude = math.max(landHeight + MinHeight, math.lerp(_cameraCoord.Altitude, _height + landHeight, Time.deltaTime * LerpSpeed));

        return new CameraState(camera, focus)
        {
            CameraLocalPosition = _cameraCoord.LocalPlanet,
            CameraLocalRotation = localCameraRotation,
            FocusLocalPosition = new Coordinate(CameraUtils.GetCursorWorldPosition(), Planet.LocalToWorld).LocalPlanet,
            FocusLocalRotation = localFocusRotation,
            FieldOfView = Fov,
        };
    }
}
