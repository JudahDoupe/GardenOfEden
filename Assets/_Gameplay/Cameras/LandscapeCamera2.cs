using System;
using System.Linq;
using Assets.GamePlay.Cameras;
using Unity.Mathematics;
using UnityEngine;

public class LandscapeCamera2 : CameraPerspective
{    [Serializable]
    private struct Settings
    {
        public float Distance;
        public float ZoomSpeed;
        public float StrafeSpeed;
        public float Fov;
    }
    
    public float RotationSpeed;
    public float LerpSpeed;
    public float PitchSpeed;
    [Range(0, 1)]
    public float PitchRange;
    [SerializeField]
    private Settings Near; 
    [SerializeField]
    private Settings Far; 
    
    private float _cameraAltitude;
    private float _cameraDistance;
    private float _cameraSetbackT;

    public override void Enable()
    {
        var cameraTransform = CurrentState.Camera.transform;
        var cameraPosition = cameraTransform.position;
        var focusPosition = CurrentState.Focus.position;
        var focusCoord = new Coordinate(focusPosition, Planet.LocalToWorld);
        focusCoord.Altitude = TerrainAltitude(focusCoord);
        
        IsActive = true;

        var up = Vector3.Normalize(focusPosition);
        var right = cameraTransform.right;
        var forward = Quaternion.AngleAxis(90, right) * up;

        CurrentState.Focus.rotation = Quaternion.LookRotation(forward, up);
        CurrentState.Focus.parent = Planet.Transform;
        CurrentState.Focus.localPosition = focusCoord.LocalPlanet;

        cameraTransform.parent = CurrentState.Focus;
        
        _cameraAltitude = cameraPosition.magnitude;
        _cameraDistance = cameraTransform.localPosition.magnitude;
        _cameraSetbackT = 0;
        cameraTransform.LookAt(focusCoord.Global(Planet.LocalToWorld),forward);


        CameraUtils.SetState(new CameraState(CurrentState.Camera, CurrentState.Focus));

    }
    public override void Disable()
    {
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        CameraUtils.SetState(GetTargetState(true));
    }

    private CameraState GetTargetState(bool lerp)
    {
        var t = Ease.Out((MinAltitude - _cameraAltitude) / (MinAltitude - MaxAltitude));
        
        var zoom = math.lerp(Near.ZoomSpeed, Far.ZoomSpeed, t) * KeyAxis(KeyCode.LeftShift, KeyCode.Space) * Time.deltaTime;
        var strafe = math.lerp(Near.StrafeSpeed, Far.StrafeSpeed, t) * new Vector2(KeyAxis(KeyCode.A, KeyCode.D), KeyAxis(KeyCode.S, KeyCode.W)) * Time.deltaTime;
        var rotation = RotationSpeed * KeyAxis(KeyCode.RightArrow, KeyCode.LeftArrow) * Time.deltaTime;
        var pitch = PitchSpeed * KeyAxis(KeyCode.UpArrow, KeyCode.DownArrow) * Time.deltaTime;
        
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
        _cameraDistance = math.clamp(_cameraDistance + zoom, Near.Distance, Far.Distance);
        _cameraSetbackT = math.clamp(_cameraSetbackT + pitch, 0.0001f, 1);
        var cameraLocalPosition = Vector3.Lerp(Vector3.up, -Vector3.forward, _cameraSetbackT * PitchRange).normalized * _cameraDistance;
        
        // Clamp Altitude
        var focusAltitude = focusLocalPosition.magnitude;
        var cameraCoord = new Coordinate(focusLocalPosition + cameraLocalPosition.z * localForward + cameraLocalPosition.y * localUp);
        _cameraAltitude = new [] {
            cameraCoord.Altitude,
            focusLocalPosition.magnitude,
            MinAltitude,
            TerrainAltitude(cameraCoord) + Camera.nearClipPlane * 1.5f,
        }.Max();
        cameraLocalPosition.y = _cameraAltitude - focusAltitude;
        
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            FocusParent = Planet.Transform,
            FocusLocalPosition = Vector3.Lerp(CurrentState.FocusLocalPosition, focusLocalPosition, lerpSpeed),
            FocusLocalRotation = Quaternion.Slerp(CurrentState.FocusLocalRotation, focusLocalRotation, lerpSpeed),
            CameraParent = CurrentState.Focus,
            CameraLocalPosition = Vector3.Lerp(CurrentState.CameraLocalPosition, cameraLocalPosition, lerpSpeed),
            CameraLocalRotation = Quaternion.Slerp(CurrentState.CameraLocalRotation ,quaternion.LookRotation(-cameraLocalPosition.normalized, Vector3.up), lerpSpeed),
            FieldOfView = math.lerp(Near.Fov, Far.Fov, t)
        };
        
        float KeyAxis(KeyCode negative, KeyCode positive) => (Input.GetKey(positive) ? 1f : 0) + (Input.GetKey(negative) ? -1f : 0);
        
    }

    float TerrainAltitude(Coordinate coord) => math.max(Planet.Data.PlateTectonics.LandHeightMap.Sample(coord).r, Planet.Data.Water.WaterMap.Sample(coord).a);
}
