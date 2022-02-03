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

    private Coordinate _coord;
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
        _coord = IsActive ? _coord : new Coordinate(CurrentState.Camera.position, Planet.LocalToWorld);
        var cameraPosition = CurrentState.Camera.localPosition;
        var t = Ease.Out((MinAltitude - cameraPosition.magnitude) / (MinAltitude - MaxAltitude));

        var right = Planet.Transform.InverseTransformDirection(CurrentState.Camera.right);
        var up = Vector3.Normalize(_coord.LocalPlanet);
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

        var altitude = math.clamp(_coord.Altitude + translation.z, MinAltitude + (IsActive ? -10 : 10), MaxAltitude - (IsActive ? -10 : 10));
        _coord.LocalPlanet += (translation.x * right + translation.y * forward).ToFloat3();
        _coord.Altitude = altitude;

        var focusPos = EnvironmentDataStore.LandHeightMap.Sample(_coord).r * up;
        var targetCameraPos = _coord.LocalPlanet.ToVector3() - forward * ((1 - math.pow(t, 2)) * Swing);
        cameraPosition = lerp ? Vector3.Lerp(cameraPosition, targetCameraPos, Time.deltaTime * LerpSpeed) : targetCameraPos;

        t = Ease.Out((MinAltitude - cameraPosition.magnitude) / (MinAltitude - MaxAltitude));
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = cameraPosition,
            CameraLocalRotation = Quaternion.LookRotation((focusPos - cameraPosition).normalized, focusPos.normalized),
            FocusParent = Planet.Transform,
            FocusLocalPosition = focusPos,
            FocusLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FieldOfView = math.lerp(Near.Fov, Far.Fov, t),
            NearClip = 10,
            FarClip = MaxAltitude + Coordinate.PlanetRadius,
        };
    }
}
