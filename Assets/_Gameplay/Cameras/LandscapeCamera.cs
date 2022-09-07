using System;
using System.Linq;
using Assets.GamePlay.Cameras;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class LandscapeCamera : CameraPerspective
{    
    [Serializable]
    private struct Settings
    {
        public float Distance;
        public float ZoomSpeed;
        public float StrafeSpeed;
        public float Fov;
    }
    
    public Texture2D CursorTexture;
    
    public float LerpSpeed;
    public float SmoothTime;
    public float RotationSpeed;
    public float PitchSpeed;
    public Vector2 DragSpeed;
    [Range(0, 1)]
    public float PitchRange;
    [SerializeField]
    private Settings Near; 
    [SerializeField]
    private Settings Far;

    private bool _isDragging;
    private float _cameraAltitude;
    private float _cameraDistance;
    private float _cameraSetbackT;
    private Controls _controls;
    
#region Transitions

    private void Start()
    {
        _controls = new Controls();
        _lastInput = new InputData()
        {
            Strafe = Vector2.zero,
            Rotation = 0,
            Pitch = 0,
            Zoom = 0,
        };
        _velocity = new InputData()
        {
            Strafe = Vector2.zero,
            Rotation = 0,
            Pitch = 0,
            Zoom = 0,
        };
    }

    public override CameraState StartTransitionTo()
    {
        //TODO: Fix this
        var currentState = CameraController.CurrentState;
        var cameraTransform = currentState.Camera.transform;
        var cameraPosition = cameraTransform.position;
        var focusPosition = currentState.Focus.position;
        var focusCoord = new Coordinate(focusPosition, Planet.LocalToWorld);
        focusCoord.Altitude = TerrainAltitude(focusCoord);

        IsActive = true;

        var up = Vector3.Normalize(focusPosition);
        var right = cameraTransform.right;
        var forward = Quaternion.AngleAxis(90, right) * up;

        currentState.Focus.rotation = Quaternion.LookRotation(forward, up);
        currentState.Focus.parent = Planet.Transform;
        currentState.Focus.localPosition = focusCoord.LocalPlanet;

        cameraTransform.parent = currentState.Focus;

        _cameraAltitude = cameraPosition.magnitude;
        _cameraDistance = cameraTransform.localPosition.magnitude;
        _cameraSetbackT = 0;
        cameraTransform.LookAt(focusCoord.Global(Planet.LocalToWorld), forward);

        CameraUtils.SetState(new CameraState(currentState.Camera, currentState.Focus));
        _controls.LandscapeCamera.Enable();
        _controls.LandscapeCamera.Click.started += c =>
        {
            var sign = (Mouse.current.position.y.ReadValue() / Screen.height) < 0.5 ? 1 : -1;
            DragSpeed.x = math.abs(DragSpeed.x) * sign;
            _isDragging = true;
        };
        _controls.LandscapeCamera.Click.canceled += _ => _isDragging = false;

        Cursor.SetCursor(CursorTexture, new Vector2(CursorTexture.width / 2f, CursorTexture.height / 2f), CursorMode.Auto);

        return currentState;
    }

    public override void Enable()
    {
        var currentState = CameraController.CurrentState;
        var cameraTransform = currentState.Camera.transform;
        var cameraPosition = cameraTransform.position;
        var focusPosition = currentState.Focus.position;
        var focusCoord = new Coordinate(focusPosition, Planet.LocalToWorld);
        focusCoord.Altitude = TerrainAltitude(focusCoord);
        
        IsActive = true;

        var up = Vector3.Normalize(focusPosition);
        var right = cameraTransform.right;
        var forward = Quaternion.AngleAxis(90, right) * up;

        currentState.Focus.rotation = Quaternion.LookRotation(forward, up);
        currentState.Focus.parent = Planet.Transform;
        currentState.Focus.localPosition = focusCoord.LocalPlanet;

        cameraTransform.parent = currentState.Focus;
        
        _cameraAltitude = cameraPosition.magnitude;
        _cameraDistance = cameraTransform.localPosition.magnitude;
        _cameraSetbackT = 0;
        cameraTransform.LookAt(focusCoord.Global(Planet.LocalToWorld),forward);

        CameraUtils.SetState(new CameraState(currentState.Camera, currentState.Focus));
        _controls.LandscapeCamera.Enable();
        _controls.LandscapeCamera.Click.started += c =>
        {
            var sign = (Mouse.current.position.y.ReadValue() / Screen.height) < 0.5 ? 1 : -1;
            DragSpeed.x = math.abs(DragSpeed.x) * sign;
            _isDragging = true;
        };
        _controls.LandscapeCamera.Click.canceled += _ => _isDragging = false;
        
        Cursor.SetCursor(CursorTexture, new Vector2(CursorTexture.width / 2f, CursorTexture.height / 2f), CursorMode.Auto);

    }
    public override void Disable()
    {
        IsActive = false;
        _controls.LandscapeCamera.Disable();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    
#endregion
    
#region Update
    
    private void LateUpdate()
    {
        if (!IsActive) return;

        CameraUtils.SetState(GetTargetState(true));
    }

    private CameraState GetTargetState(bool lerp)
    {
        var currentState = CameraController.CurrentState;

        var t = Ease.Out((MinAltitude - _cameraAltitude) / (MinAltitude - MaxAltitude));
        var input = GetInput();

        var localUp = Vector3.Normalize(currentState.FocusLocalPosition);
        var localRight = Planet.Transform.InverseTransformDirection(currentState.Focus.right);
        var localForward = Quaternion.AngleAxis(90, localRight) * localUp;

        var lerpSpeed = lerp ? Time.deltaTime * LerpSpeed : 1;
        
        // Focus Position
        var focusLocalPosition = currentState.FocusLocalPosition;
        focusLocalPosition += localRight * input.Strafe.x + localForward * input.Strafe.y;
        var focusCoord = new Coordinate(focusLocalPosition);
        focusCoord.Altitude = TerrainAltitude(focusCoord);
        focusLocalPosition = focusCoord.LocalPlanet;
        
        // Focus Rotation
        var focusLocalRotation = Quaternion.LookRotation(localForward, localUp);
        focusLocalRotation = Quaternion.AngleAxis(input.Rotation, localUp) * focusLocalRotation;

        // Camera Position
        _cameraDistance = math.clamp(_cameraDistance + input.Zoom, Near.Distance, Far.Distance);
        _cameraSetbackT = math.clamp(_cameraSetbackT + input.Pitch, 0.0001f, 1);
        var cameraLocalPosition = Vector3.Lerp(Vector3.up, -Vector3.forward, _cameraSetbackT * PitchRange).normalized * _cameraDistance;
        
        // Clamp Altitude
        var focusAltitude = focusLocalPosition.magnitude;
        var cameraCoord = new Coordinate(focusLocalPosition + cameraLocalPosition.z * localForward + cameraLocalPosition.y * localUp);
        _cameraAltitude = new [] {
            cameraCoord.Altitude,
            focusLocalPosition.magnitude,
            MinAltitude,
            TerrainAltitude(cameraCoord) + currentState.Camera.nearClipPlane * 1.5f,
        }.Max();
        cameraLocalPosition.y = _cameraAltitude - focusAltitude;
        
        return new CameraState(currentState.Camera, currentState.Focus)
        {
            FocusParent = Planet.Transform,
            FocusLocalPosition = Vector3.Lerp(currentState.FocusLocalPosition, focusLocalPosition, lerpSpeed),
            FocusLocalRotation = Quaternion.Slerp(currentState.FocusLocalRotation, focusLocalRotation, lerpSpeed),
            CameraParent = currentState.Focus,
            CameraLocalPosition = Vector3.Lerp(currentState.CameraLocalPosition, cameraLocalPosition, lerpSpeed),
            CameraLocalRotation = Quaternion.Slerp(currentState.CameraLocalRotation ,quaternion.LookRotation(-cameraLocalPosition.normalized, Vector3.up), lerpSpeed),
            FieldOfView = math.lerp(Near.Fov, Far.Fov, t)
        };
    }

    float TerrainAltitude(Coordinate coord) => math.max(Planet.Data.PlateTectonics.LandHeightMap.Sample(coord).r, Planet.Data.Water.WaterMap.Sample(coord).a);
    
#endregion
    
#region Input

    private struct InputData 
    {
        public Vector2 Strafe;
        public float Rotation;
        public float Pitch;
        public float Zoom;
    };
    private InputData _lastInput;
    private InputData _velocity;
    private InputData GetInput()
    {
        var t = Ease.Out((MinAltitude - _cameraAltitude) / (MinAltitude - MaxAltitude));
        var drag = Mouse.current.delta.ReadValue() * DragSpeed * Convert.ToInt16(_isDragging);
        var zoom = math.lerp(Near.ZoomSpeed, Far.ZoomSpeed, t) * _controls.LandscapeCamera.Zoom.ReadValue<float>() * Time.deltaTime;
        var strafe = math.lerp(Near.StrafeSpeed, Far.StrafeSpeed, t) * _controls.LandscapeCamera.Strafe.ReadValue<Vector2>() * Time.deltaTime;
        var rotation = RotationSpeed * (drag.x + _controls.LandscapeCamera.Rotate.ReadValue<float>()) * Time.deltaTime;
        var pitch = PitchSpeed * (drag.y + _controls.LandscapeCamera.Pitch.ReadValue<float>()) * Time.deltaTime;

        _lastInput = new InputData
        {
            Strafe = Vector2.SmoothDamp(_lastInput.Strafe, strafe, ref _velocity.Strafe, SmoothTime),
            Rotation = Mathf.SmoothDamp(_lastInput.Rotation, rotation, ref _velocity.Rotation, SmoothTime),
            Pitch = Mathf.SmoothDamp(_lastInput.Pitch, pitch, ref _velocity.Pitch, SmoothTime),
            Zoom = Mathf.SmoothDamp(_lastInput.Zoom, zoom, ref _velocity.Zoom, SmoothTime),
        };
        return _lastInput;
    }

#endregion
}
