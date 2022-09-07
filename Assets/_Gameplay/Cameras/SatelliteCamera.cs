using Assets.GamePlay.Cameras;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class SatelliteCamera : CameraPerspective
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
    public float PoleBuffer = 30;
    public bool IsDragEnabled = true;
    public Texture2D CursorTexture;
    [SerializeField]
    private Settings Near; 
    [SerializeField]
    private Settings Far; 

    private Coordinate _coord;
    private Controls _controls;
    private bool _isDragging;

    #region Transition
    #endregion

    public override CameraState StartTransitionTo() => GetTargetState(false);

    public override void Enable()
    {
        _controls = new Controls();
        _controls.SateliteCamera.Enable();
        _controls.SateliteCamera.Click.started += _ => _isDragging = true;
        _controls.SateliteCamera.Click.canceled += _ => _isDragging = false;
        Cursor.SetCursor(CursorTexture, new Vector2(CursorTexture.width / 2f, CursorTexture.height / 2f), CursorMode.Auto);
        IsActive = true;
    }
    public override void Disable()
    {
        _controls.SateliteCamera.Disable();
        _controls.Dispose();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        CameraUtils.SetState(GetTargetState(true));
    }

    private CameraState GetTargetState(bool lerp)
    {
        var currentState = CameraController.CurrentState;
        _coord = IsActive ? _coord : new Coordinate(currentState.Camera.transform.position, Planet.LocalToWorld);
        var cameraPosition = currentState.Camera.transform.localPosition;
        var t = Ease.Out((MinAltitude - _coord.Altitude) / (MinAltitude - MaxAltitude));
        var translation = Vector3.zero;
        if (IsActive)
        {
            var z =  math.lerp(Near.ZoomSpeed, Far.ZoomSpeed, t) * Coordinate.PlanetRadius;
            var m = math.lerp(Near.MovementSpeed, Far.MovementSpeed, t) * Time.deltaTime;
            var movement = _isDragging && IsDragEnabled 
                ? Mouse.current.delta.ReadValue() * -DragSpeed
                : _controls.SateliteCamera.Rotate.ReadValue<Vector2>();
            var zoom = _controls.SateliteCamera.Zoom.ReadValue<float>();
            translation = new Vector3(movement.x * m, movement.y * -m, -zoom * z);
        }

        _coord.Altitude = math.clamp(_coord.Altitude + translation.z, MinAltitude + (IsActive ? -10 : 10), MaxAltitude - (IsActive ? -10 : 10));
        _coord.Lat = math.clamp(_coord.Lat + translation.y, PoleBuffer, 180 - PoleBuffer);
        _coord.Lon += translation.x;

        cameraPosition = lerp ? Vector3.Lerp(cameraPosition, _coord.LocalPlanet, Time.deltaTime * LerpSpeed) : _coord.LocalPlanet;
        t = Ease.Out((MinAltitude - cameraPosition.magnitude) / (MinAltitude - MaxAltitude));
        var height = Planet.Data.PlateTectonics.LandHeightMap.Sample(_coord).r;
        return new CameraState(currentState.Camera, currentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = cameraPosition,
            CameraLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FocusParent = Planet.Transform,
            FocusLocalPosition = height * cameraPosition.normalized,
            FocusLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FieldOfView = math.lerp(Near.Fov, Far.Fov, t),
            NearClip = 10,
            FarClip = MaxAltitude + Coordinate.PlanetRadius,
        };
    }
}
