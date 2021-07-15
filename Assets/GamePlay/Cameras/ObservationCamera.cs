using Unity.Mathematics;
using UnityEngine;

public class ObservationCamera : MonoBehaviour
{
    [Header("Altitude")]
    public float LerpSpeed = 2f;
    public float MaxHeight = 100;
    public float MinHeight = 1;
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

    private CameraState _currentState;
    private Coordinate _cameraCoord;
    private float _height;

    public void Enable(CameraState currentState)
    {
        _currentState = GetTargetState(currentState, true);
        Cursor.lockState = CursorLockMode.Locked;
        IsActive = true;
    }

    public void Disable()
    {
        Cursor.lockState = CursorLockMode.None;
        IsActive = false;
    }

    private void Update()
    {
        if (!IsActive) return;

        var ray = new Ray(_currentState.Camera.position, _currentState.Camera.forward);
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

        _currentState = GetTargetState(_currentState, true);
        CameraUtils.SetState(_currentState);

        if (_height > MaxHeight)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
    }

    public CameraState GetTargetState(CameraState currentState, bool lerp)
    {
        if (!IsActive)
        {
            _cameraCoord = new Coordinate(currentState.Camera.position, Planet.LocalToWorld);
            _height = math.min(_cameraCoord.Altitude - Singleton.Land.SampleHeight(_cameraCoord), MaxHeight);
        }

        // Calculate movement
        var t = (MaxHeight - _height) / (MaxHeight - MinHeight);
        var translation = IsActive 
            ? new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * math.lerp(MaxMovementSpeed, MinMovementSpeed, t)
            : Vector3.zero;
        var rotation = IsActive 
            ? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * RotationSpeed
            : Vector2.zero;
        var zoom = IsActive 
            ? - Input.mouseScrollDelta.y * math.lerp(MaxZoomSpeed, MinZoomSpeed, t)
            : 0;

        // Calculate orientation
        var right = currentState.Camera.right;
        var up = currentState.Camera.position.normalized;
        var forward = Quaternion.AngleAxis(90, right) * up;

        // Calculate rotation
        var cameraRotation = Quaternion.LookRotation(currentState.Camera.forward, up);
        cameraRotation = Quaternion.AngleAxis(rotation.x, up) * cameraRotation;
        right = currentState.Camera.right;
        var desiredVerticalRotation = Quaternion.AngleAxis(rotation.y, right) * cameraRotation;
        var desiredAngle = Quaternion.Angle(quaternion.LookRotation(forward, up), desiredVerticalRotation);
        var currentAngle = Quaternion.Angle(quaternion.LookRotation(forward, up), cameraRotation);
        if (desiredAngle <= VerticalAngle || desiredAngle < currentAngle)
        {
            cameraRotation = desiredVerticalRotation;
        }
        var localCameraRotation = Quaternion.Inverse(Planet.Transform.rotation) * cameraRotation;
        var localFocusRotation = Quaternion.Inverse(Planet.Transform.rotation) * quaternion.LookRotation(forward, up);

        // Calculate position
        _height = math.max(MinHeight, _height + zoom);
        var landHeight = Singleton.Land.SampleHeight(_cameraCoord);
        var targetAltitude = lerp ? math.lerp(_cameraCoord.Altitude, _height + landHeight, Time.deltaTime * LerpSpeed) : _height + landHeight;
        targetAltitude = math.max(landHeight + MinHeight, targetAltitude);
        if (IsActive)
        {
            var altitudeDifference = targetAltitude - _cameraCoord.Altitude;
            translation.z -= altitudeDifference;
        }
        _cameraCoord.LocalPlanet += (localFocusRotation * translation).ToFloat3();
        _cameraCoord.Altitude = math.max(landHeight + MinHeight, targetAltitude);

        return new CameraState(currentState.Camera, currentState.Focus)
        {
            CameraLocalPosition = _cameraCoord.LocalPlanet,
            CameraLocalRotation = localCameraRotation,
            FocusLocalPosition = new Coordinate(CameraUtils.GetCursorWorldPosition(), Planet.LocalToWorld).LocalPlanet,
            FocusLocalRotation = localFocusRotation,
            FieldOfView = Fov,
        };
    }
}
