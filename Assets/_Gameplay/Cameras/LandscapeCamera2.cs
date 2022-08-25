using System;
using System.Linq;
using Assets.GamePlay.Cameras;
using Unity.Mathematics;
using UnityEngine;

public class LandscapeCamera2 : CameraPerspective
{
    public float StrafeSpeed;
    public float RotationSpeed;
    public float PitchSpeed;
    public float ZoomSpeed;
    public float LerpSpeed;
    public float SetbackMultiplier;
    
    private Controls _controls;
    private bool _isDragging;
    private float _cameraAltitude;
    private float _cameraSetback;

    public override void Enable()
    {
        var cameraPosition = CurrentState.Camera.transform.position;
        var focusPosition = CurrentState.Focus.position;
        var focusCoord = new Coordinate(focusPosition, Planet.LocalToWorld);
        focusCoord.Altitude = TerrainAltitude(focusCoord);

        _controls = new Controls();
        _controls.SateliteCamera.Enable();
        _controls.SateliteCamera.Click.started += context => _isDragging = true;
        _controls.SateliteCamera.Click.canceled += context => _isDragging = false;
        IsActive = true;

        var forward = ((cameraPosition - CurrentState.Camera.transform.up) - focusPosition.normalized).normalized;
        var up = focusPosition.normalized;

        CurrentState.Focus.rotation = Quaternion.LookRotation(forward, up);
        CurrentState.Focus.parent = Planet.Transform;
        CurrentState.Focus.localPosition = focusCoord.LocalPlanet;

        CurrentState.Camera.transform.parent = CurrentState.Focus;
        
        _cameraAltitude = cameraPosition.magnitude;
        _cameraSetback = Vector3.Project(cameraPosition, forward).magnitude;

        CameraUtils.SetState(new CameraState(CurrentState.Camera, CurrentState.Focus));

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
        var strafe = StrafeSpeed * new Vector2(KeyAxis(KeyCode.A, KeyCode.D), KeyAxis(KeyCode.S, KeyCode.W));
        var rotation = RotationSpeed * KeyAxis(KeyCode.LeftArrow, KeyCode.RightArrow);
        var pitch = PitchSpeed * KeyAxis(KeyCode.UpArrow, KeyCode.DownArrow);
        var zoom = ZoomSpeed * KeyAxis(KeyCode.LeftShift, KeyCode.Space);
        
        var localUp = Vector3.Normalize(CurrentState.FocusLocalPosition);
        var localRight = Planet.Transform.InverseTransformDirection(CurrentState.Focus.right);
        var localForward = Quaternion.AngleAxis(90, localRight) * localUp;

        var lerpSpeed = lerp ? Time.deltaTime * LerpSpeed : 1;
        
        // Focus Position
        var focusLocalPosition = CurrentState.FocusLocalPosition;
        focusLocalPosition += localRight * strafe.x + localForward * strafe.y;
        var focusCoord = new Coordinate(focusLocalPosition);
        focusCoord.Altitude = TerrainAltitude(focusCoord);
        focusLocalPosition = focusCoord.LocalPlanet;
        
        // Focus Rotation
        var focusLocalRotation = Quaternion.LookRotation(localForward, localUp);
        focusLocalRotation = Quaternion.AngleAxis(rotation, localUp) * focusLocalRotation;

        // Camera Position
        var focusAltitude = focusLocalPosition.magnitude;
        var cameraCoord = new Coordinate(focusLocalPosition - _cameraSetback * localForward);
        _cameraAltitude = new [] {
            _cameraAltitude + zoom,
            focusLocalPosition.magnitude,
            MinAltitude,
            TerrainAltitude(cameraCoord) + Camera.nearClipPlane * 1.5f,
        }.Max();
        _cameraAltitude = new [] {
            _cameraAltitude,
            MaxAltitude
        }.Min();
        _cameraSetback = math.clamp(_cameraSetback + pitch, 0.1f, (_cameraAltitude - focusAltitude) * SetbackMultiplier);
        var cameraLocalPosition = new Vector3(0, _cameraAltitude - focusAltitude, -_cameraSetback);
        
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            FocusParent = Planet.Transform,
            FocusLocalPosition = Vector3.Lerp(CurrentState.FocusLocalPosition, focusLocalPosition, lerpSpeed),
            FocusLocalRotation = Quaternion.Slerp(CurrentState.FocusLocalRotation, focusLocalRotation, lerpSpeed),
            CameraParent = CurrentState.Focus,
            CameraLocalPosition = Vector3.Lerp(CurrentState.CameraLocalPosition, cameraLocalPosition, lerpSpeed),
            CameraLocalRotation = Quaternion.Slerp(CurrentState.CameraLocalRotation ,quaternion.LookRotation(-cameraLocalPosition.normalized, Vector3.up), lerpSpeed),
        };
        
        float KeyAxis(KeyCode negative, KeyCode positive) => (Input.GetKey(positive) ? 1f : 0) + (Input.GetKey(negative) ? -1f : 0);
    }

    float TerrainAltitude(Coordinate coord) => math.max(Planet.Data.PlateTectonics.LandHeightMap.Sample(coord).r, Planet.Data.Water.WaterMap.Sample(coord).a);
}
