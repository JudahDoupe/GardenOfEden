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
    
    private Controls _controls;
    private bool _isDragging;
    private float _cameraAltitude;
    private float _cameraSetback;

    public override void Enable()
    {
        var cameraPosition = CurrentState.Camera.transform.position;
        var focusPosition = CurrentState.Focus.position;
        var focusCoord = new Coordinate(focusPosition, Planet.LocalToWorld);
        focusCoord.Altitude = Planet.Data.PlateTectonics.LandHeightMap.Sample(focusCoord).r;

        _cameraAltitude = cameraPosition.magnitude;
        _cameraSetback = math.max(0.01f, Vector3.Distance(focusPosition, cameraPosition.normalized * focusPosition.magnitude));
        
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
        var strafe = StrafeSpeed * new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
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
        focusCoord.Altitude = Planet.Data.PlateTectonics.LandHeightMap.Sample(focusCoord).r;
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
            Planet.Data.PlateTectonics.LandHeightMap.Sample(cameraCoord).r + Camera.nearClipPlane * 1.5f, 
            Planet.Data.Water.WaterMap.Sample(cameraCoord).a + Camera.nearClipPlane * 1.5f, 
        }.Max();
        _cameraAltitude = new [] {
            _cameraAltitude,
            MaxAltitude
        }.Min();
        _cameraSetback = math.clamp(_cameraSetback + pitch, 0.1f, _cameraAltitude - focusAltitude);
        var cameraLocalPosition = new Vector3(0, _cameraAltitude - focusAltitude, -_cameraSetback);
        
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            FocusParent = Planet.Transform,
            FocusLocalPosition = Vector3.Lerp(CurrentState.FocusLocalPosition, focusLocalPosition, lerpSpeed),
            FocusLocalRotation = Quaternion.Slerp(CurrentState.FocusLocalRotation, focusLocalRotation, lerpSpeed),
            CameraParent = CurrentState.Focus,
            CameraLocalPosition = Vector3.Lerp(CurrentState.CameraLocalPosition, cameraLocalPosition, lerpSpeed),
            CameraLocalRotation = Quaternion.Slerp(CurrentState.CameraLocalRotation ,quaternion.LookRotation((focusLocalPosition - cameraLocalPosition).normalized, localUp), lerpSpeed),
        };
        
        float KeyAxis(KeyCode negative, KeyCode positive) => (Input.GetKey(positive) ? 1f : 0) + (Input.GetKey(negative) ? -1f : 0);
    }


}
