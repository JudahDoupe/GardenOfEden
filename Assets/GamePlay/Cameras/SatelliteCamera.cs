using Assets.GamePlay.Cameras;
using System;
using System.Collections;
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
    public float LerpSpeed = 5f;
    public float PoleBuffer = 30;
    [SerializeField]
    private Settings Near; 
    [SerializeField]
    private Settings Far; 

    private Coordinate _coord;
    private Controls _controls;

    public override CameraState TransitionToState() => GetTargetState(false);

    public override void Enable()
    {
        _controls = new Controls();
        _controls.SateliteCamera.Enable();
    }
    public override void Disable()
    {
        _controls.SateliteCamera.Disable();
        _controls.Dispose();
    }


    private void LateUpdate()
    {
        if (!IsActive) return;

        CameraUtils.SetState(GetTargetState(true));
    }

    private IEnumerator DragCamera()
    {
        yield return new WaitForEndOfFrame();
    }

    public CameraState GetTargetState(bool lerp)
    {
        _coord = IsActive ? _coord : new Coordinate(CurrentState.Camera.position, Planet.LocalToWorld);
        var cameraPosition = CurrentState.Camera.localPosition;
        var t = Ease.Out((MinAltitude - _coord.Altitude) / (MinAltitude - MaxAltitude));
        var z =  math.lerp(Near.ZoomSpeed, Far.ZoomSpeed, t) * Coordinate.PlanetRadius;
        var m = math.lerp(Near.MovementSpeed, Far.MovementSpeed, t) * Time.deltaTime;
        var movement = _controls.SateliteCamera.Rotate.ReadValue<Vector2>();
        var zoom = _controls.SateliteCamera.Zoom.ReadValue<float>();
        var translation = IsActive 
            ? new Vector3(movement.x * m, movement.y * -m, -zoom * z)
            : Vector3.zero;

        _coord.Altitude = math.clamp(_coord.Altitude + translation.z, MinAltitude + (IsActive ? -10 : 10), MaxAltitude - (IsActive ? -10 : 10));
        _coord.Lat = math.clamp(_coord.Lat + translation.y, PoleBuffer, 180 - PoleBuffer);
        _coord.Lon += translation.x;

        cameraPosition = lerp ? Vector3.Lerp(cameraPosition, _coord.LocalPlanet, Time.deltaTime * LerpSpeed) : (Vector3)_coord.LocalPlanet;
        t = Ease.Out((MinAltitude - cameraPosition.magnitude) / (MinAltitude - MaxAltitude));
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = cameraPosition,
            CameraLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FocusParent = Planet.Transform,
            FocusLocalPosition = EnvironmentDataStore.LandHeightMap.Sample(_coord).r * cameraPosition.normalized,
            FocusLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FieldOfView = math.lerp(Near.Fov, Far.Fov, t),
            NearClip = 10,
            FarClip = MaxAltitude + Coordinate.PlanetRadius,
        };
    }
}
