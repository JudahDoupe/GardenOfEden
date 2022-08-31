using Assets.GamePlay.Cameras;
using Unity.Mathematics;
using UnityEngine;

public class ObservationCamera : CameraPerspective
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

    private Coordinate _cameraCoord;
    private float _height;

    public override void Enable()
    {
        IsActive = true;
        _cameraCoord = new Coordinate(CameraController.CurrentState.Camera.transform.position, Planet.LocalToWorld);
        _height = math.min(_cameraCoord.Altitude - Planet.Data.PlateTectonics.LandHeightMap.Sample(_cameraCoord).r, MaxHeight -1);
    }

    public override CameraState TransitionToState() => GetTargetState(false);

    private void LateUpdate()
    {
        if (!IsActive) return;

        CameraUtils.SetState(GetTargetState(true));
    }

    public CameraState GetTargetState(bool lerp)
    {
        return IsActive ? GetActiveTargetState(lerp) : GetInactiveTargetState();
    }

    private CameraState GetActiveTargetState(bool lerp)
    {
        var currentState = CameraController.CurrentState;

        // Calculate movement
        var t = (MaxHeight - _height) / (MaxHeight - MinHeight);
        var translation = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * math.lerp(MaxMovementSpeed, MinMovementSpeed, t);
        var rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * RotationSpeed;
        var zoom = -Input.mouseScrollDelta.y * math.lerp(MaxZoomSpeed, MinZoomSpeed, t);

        // Calculate orientation
        var cameraTransform = currentState.Camera.transform;
        var right = cameraTransform.right;
        var up = cameraTransform.position.normalized;
        var forward = Quaternion.AngleAxis(90, right) * up;

        // Calculate rotation
        var cameraRotation = Quaternion.LookRotation(currentState.Camera.transform.forward, up);
        cameraRotation = Quaternion.AngleAxis(rotation.x, up) * cameraRotation;
        right = currentState.Camera.transform.right;
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
        var landHeight = Planet.Data.PlateTectonics.LandHeightMap.Sample(_cameraCoord).r;
        var targetAltitude = lerp ? math.lerp(_cameraCoord.Altitude, _height + landHeight, Time.deltaTime * LerpSpeed) : _height + landHeight;
        var changeInAltitude = _cameraCoord.Altitude - math.max(landHeight + MinHeight, targetAltitude);
        translation.z += changeInAltitude;
        _cameraCoord.LocalPlanet += (localFocusRotation * translation).ToFloat3();
        _cameraCoord.Altitude = math.max(landHeight + MinHeight, targetAltitude);

        return new CameraState(currentState.Camera, currentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = _cameraCoord.LocalPlanet,
            CameraLocalRotation = localCameraRotation,
            FocusParent = Planet.Transform,
            FocusLocalPosition = new Coordinate(CameraUtils.GetCursorWorldPosition(), Planet.LocalToWorld).LocalPlanet,
            FocusLocalRotation = localFocusRotation,
            FieldOfView = Fov,
            Cursor = CursorLockMode.Locked,
            NearClip = 0.1f,
            FarClip = MaxHeight + Coordinate.PlanetRadius,
        };
    }

    private CameraState GetInactiveTargetState()
    {
        var currentState = CameraController.CurrentState;

        var cameraTransform = currentState.Camera.transform;
        _cameraCoord = new Coordinate(cameraTransform.position, Planet.LocalToWorld);
        _height = math.clamp(_cameraCoord.Altitude - Planet.Data.PlateTectonics.LandHeightMap.Sample(_cameraCoord).r, MinHeight, MaxHeight);

        // Calculate orientation
        var right = cameraTransform.right;
        var up = cameraTransform.position.normalized;
        var forward = Quaternion.AngleAxis(90, right) * up;

        // Calculate position
        var focusPosition = currentState.Focus.position;
        var cameraPosition = focusPosition - forward * _height + up * (_height * 2f / 3f);
        _cameraCoord = new Coordinate(focusPosition - forward * _height + up * _height, Planet.LocalToWorld);

        // Calculate rotation
        var cameraRotation = Quaternion.LookRotation((focusPosition - cameraPosition).normalized, up);
        var localCameraRotation = Quaternion.Inverse(Planet.Transform.rotation) * cameraRotation;
        var localFocusRotation = Quaternion.Inverse(Planet.Transform.rotation) * quaternion.LookRotation(forward, up);

        return new CameraState(currentState.Camera, currentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = _cameraCoord.LocalPlanet,
            CameraLocalRotation = localCameraRotation,
            FocusParent = Planet.Transform,
            FocusLocalPosition = new Coordinate(CameraUtils.GetCursorWorldPosition(), Planet.LocalToWorld).LocalPlanet,
            FocusLocalRotation = localFocusRotation,
            FieldOfView = Fov,
            Cursor = CursorLockMode.Locked,
            NearClip = 0.1f,
            FarClip = MaxHeight + Coordinate.PlanetRadius,
        };
    }
}
