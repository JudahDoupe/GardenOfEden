using Assets.GamePlay.Cameras;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class LandscapeCamera : CameraPerspective
{
    [Serializable]
    private struct Settings
    {
        public float MovementSpeed;
        public float ZoomSpeed;
        public float Fov;
    }
    public float DragSpeed = 0.25f;
    public float LerpSpeed = 5f;
    public float Swing = 100;
    public bool IsDragEnabled = true;
    [SerializeField]
    private Settings Near; 
    [SerializeField]
    private Settings Far; 

    private Coordinate _centerCoord;
    private Controls _controls;
    private bool _isDragging;

    public override CameraState TransitionToState() => GetTargetState(false);

    public override void Enable()
    {
        _controls = new Controls();
        _controls.SateliteCamera.Enable();
        _controls.SateliteCamera.Click.started += context => _isDragging = true;
        _controls.SateliteCamera.Click.canceled += context => _isDragging = false;
        IsActive = true;
    }
    public override void Disable()
    {
        _controls.SateliteCamera.Disable();
        _controls.Dispose();
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        CameraUtils.SetState(GetTargetState(true));

        if (Altitude < MinAltitude) Singleton.PerspectiveController.ZoomIn();
        if (MaxAltitude < Altitude) Singleton.PerspectiveController.ZoomOut();
    }

    private CameraState GetTargetState(bool lerp)
    {
        _centerCoord = IsActive ? _centerCoord : new Coordinate(CurrentState.Camera.position, Planet.LocalToWorld);
        var cameraPos = CurrentState.Camera.localPosition;
        var focusPos = CurrentState.FocusLocalPosition;
        var t = Ease.Out((MinAltitude - cameraPos.magnitude) / (MinAltitude - MaxAltitude));

        var right = Planet.Transform.InverseTransformDirection(CurrentState.Camera.right);
        var up = Vector3.Normalize(_centerCoord.LocalPlanet);
        var forward = Quaternion.AngleAxis(90, right) * up;

        var translation = Vector3.zero;
        if (IsActive)
        {
            var z =  math.lerp(Near.ZoomSpeed, Far.ZoomSpeed, t) * Coordinate.PlanetRadius;
            var m = math.lerp(Near.MovementSpeed, Far.MovementSpeed, t) * Time.deltaTime;
            var movement = _isDragging && IsDragEnabled 
                ? Mouse.current.delta.ReadValue() * -DragSpeed
                : _controls.SateliteCamera.Rotate.ReadValue<Vector2>();
            var zoom = _controls.SateliteCamera.Zoom.ReadValue<float>();
            translation = new Vector3(movement.x * m, movement.y * m, -zoom * z);
        }

        _centerCoord.TravelArc(translation.x, right);
        _centerCoord.TravelArc(translation.y, forward);
        _centerCoord.Altitude = math.clamp(_centerCoord.Altitude + translation.z, MinAltitude + (IsActive ? -10 : 10), MaxAltitude - (IsActive ? -10 : 10));

        var center = _centerCoord.LocalPlanet.ToVector3();
        var swing = math.clamp(1 - math.pow(t, 2), 0.001f, 1) * Swing;

        var targetFocusPos = EnvironmentMapDataStore.LandHeightMap.Sample(_centerCoord).r * center.normalized;
        focusPos = lerp ? Vector3.Lerp(focusPos, targetFocusPos, Time.deltaTime * LerpSpeed) : targetFocusPos;

        var targetCameraPos = center - forward * swing;
        cameraPos = lerp ? Vector3.Lerp(cameraPos, targetCameraPos, Time.deltaTime * LerpSpeed) : targetCameraPos;

        t = Ease.Out((MinAltitude - cameraPos.magnitude) / (MinAltitude - MaxAltitude));
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = cameraPos,
            CameraLocalRotation = Quaternion.LookRotation((focusPos - cameraPos).normalized, focusPos.normalized),
            FocusParent = Planet.Transform,
            FocusLocalPosition = focusPos,
            FocusLocalRotation = Quaternion.LookRotation(-cameraPos.normalized, Vector3.up),
            FieldOfView = math.lerp(Near.Fov, Far.Fov, t),
            NearClip = 10,
            FarClip = MaxAltitude + Coordinate.PlanetRadius,
        };
    }
}
