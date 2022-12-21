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
    
    private float _t => (_targetInput.CameraDistance - Near.Distance) / (Far.Distance - Near.Distance);
    private float _zoomSpeed => math.lerp(Near.ZoomSpeed, Far.ZoomSpeed, _t);
    private float _strafeSpeed => math.lerp(Near.StrafeSpeed, Far.StrafeSpeed, _t);
    
    public Texture2D CursorTexture;
    
    
    public float LerpSpeed;
    public float SmoothSpeed;
    public float RotationSpeed;
    public float PitchSpeed;
    public float MinAngle = -20;
    public float MaxAngle = 80;
    [SerializeField]
    private Settings Near; 
    [SerializeField]
    private Settings Far;

    private int _horizontalDragDirection;
    private bool _isDragging;

    private struct Input
    {
        public float CameraDistance;
        public float CameraAngle;
        public float CameraAltitude;
        public Vector3 FocusLocalPosition;
        public Quaternion FocusLocalRotation;
    }
    private Input _targetInput;
    private Input _currentInput;
    private Input _velocity;
    

    public override CameraState StartTransitionTo()
    {
        Cursor.SetCursor(CursorTexture, new Vector2(CursorTexture.width / 2f, CursorTexture.height / 2f), CursorMode.Auto);

        _currentInput = new Input();

        var focusPosition = CurrentState.Focus.position;
        var focusCoord = new Coordinate(focusPosition, Planet.LocalToWorld).ClampToTerrain();
        
        var localUp = Vector3.Normalize(focusCoord.LocalPlanet);
        var localRight = Planet.Transform.InverseTransformDirection(CurrentState.Camera.transform.right);
        var localForward = Quaternion.AngleAxis(90, localRight) * localUp;

        _currentInput.FocusLocalPosition = focusCoord.LocalPlanet;
        _currentInput.FocusLocalRotation = quaternion.LookRotation(localForward, localUp);

        var cameraBack = Quaternion.AngleAxis(_targetInput.CameraAngle, localRight) * -localForward;
        var cameraCoord = new Coordinate(_currentInput.FocusLocalPosition + cameraBack * _targetInput.CameraDistance);
        var maxBack = Quaternion.AngleAxis(MaxAngle, localRight) * -localForward;
        var maxCameraCoord = new Coordinate(_currentInput.FocusLocalPosition + maxBack * Far.Distance);
        cameraCoord.Altitude = math.clamp(cameraCoord.Altitude,
            TerrainAltitude(cameraCoord),
            maxCameraCoord.Altitude);
        _currentInput.CameraAltitude = math.clamp(cameraCoord.Altitude, MinAltitude, maxCameraCoord.Altitude);
        
        var cameraVector = _targetInput.FocusLocalPosition - cameraCoord.LocalPlanet.ToVector3();
        _currentInput.CameraAngle = math.clamp(Vector3.Angle(cameraVector.normalized, localForward), MinAngle, MaxAngle);
        _currentInput.CameraDistance = math.clamp(cameraVector.magnitude, Near.Distance, Far.Distance);
       
        _targetInput = new Input
        {
            CameraDistance = _currentInput.CameraDistance,
            CameraAngle = _currentInput.CameraAngle,
            CameraAltitude = _currentInput.CameraAltitude,
            FocusLocalPosition = _currentInput.FocusLocalPosition,
            FocusLocalRotation = _currentInput.FocusLocalRotation,
        };

        return GetTargetState(_currentInput);
    }

    public override void Enable()
    {
        IsActive = true;

        InputAdapter.LeftMove.Subscribe(this);
        InputAdapter.RightMove.Subscribe(this);
        InputAdapter.MoveModifier.Subscribe(this);
        InputAdapter.Scroll.Subscribe(this,
            callback: delta =>
            {
                _targetInput.CameraDistance = math.clamp(_targetInput.CameraDistance + delta * _zoomSpeed, Near.Distance, Far.Distance);

                RecalculateTargetAltitude();
            });
        InputAdapter.Click.Subscribe(this,
            startCallback: () =>
            {
                _horizontalDragDirection = Convert.ToInt32(Mouse.current.position.ReadValue().y < (Screen.height / 2.0)) * 2 - 1;
                _isDragging = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            },
            finishCallback: () =>
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                _isDragging = false;
            },

            priority: InputPriority.Low);
        InputAdapter.Drag.Subscribe(this, 
            priority: InputPriority.Low,
            callback: delta =>
            {
                if (!_isDragging)
                    return;
                
                var rotation = RotationSpeed * delta.x * _horizontalDragDirection;
                _targetInput.FocusLocalRotation = Quaternion.AngleAxis(rotation, Vector3.Normalize(_targetInput.FocusLocalPosition)) * _targetInput.FocusLocalRotation;

                var pitch = PitchSpeed * delta.y;
                _targetInput.CameraAngle = math.clamp(_targetInput.CameraAngle + pitch, MinAngle, MaxAngle);

                RecalculateTargetAltitude();
            });
        
        void RecalculateTargetAltitude()
        {
            var localUp = Vector3.Normalize(_targetInput.FocusLocalPosition);
            var localRight = _targetInput.FocusLocalRotation * Vector3.right;
            var localForward = Quaternion.AngleAxis(90, localRight) * localUp;
            var cameraBack = Quaternion.AngleAxis(_targetInput.CameraAngle, localRight) * -localForward;
            _targetInput.CameraAltitude = (CurrentState.FocusLocalPosition + cameraBack * _targetInput.CameraDistance).magnitude;
        }
    }
    
    public override void Disable()
    {
        IsActive = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        InputAdapter.LeftMove.Unsubscribe(this);
        InputAdapter.RightMove.Unsubscribe(this);
        InputAdapter.MoveModifier.Unsubscribe(this);
        InputAdapter.Scroll.Unsubscribe(this);
        InputAdapter.Click.Unsubscribe(this);
        InputAdapter.Drag.Unsubscribe(this);
    }
    
    private void LateUpdate()
    {
        if (!IsActive) return;

        Move();
        
        var lerpSpeed = Time.deltaTime * LerpSpeed;
        _currentInput = new Input
        {
            CameraDistance = Mathf.SmoothDamp(_currentInput.CameraDistance, _targetInput.CameraDistance, ref _velocity.CameraDistance, SmoothSpeed),
            CameraAngle = Mathf.SmoothDamp(_currentInput.CameraAngle, _targetInput.CameraAngle, ref _velocity.CameraAngle, SmoothSpeed),
            CameraAltitude = Mathf.SmoothDamp(_currentInput.CameraAltitude, _targetInput.CameraAltitude, ref _velocity.CameraAltitude, SmoothSpeed),
            FocusLocalPosition = Vector3.SmoothDamp(_currentInput.FocusLocalPosition, _targetInput.FocusLocalPosition, ref _velocity.FocusLocalPosition, SmoothSpeed),
            FocusLocalRotation = Quaternion.Slerp(_currentInput.FocusLocalRotation, _targetInput.FocusLocalRotation, lerpSpeed),
        };
        
        CameraUtils.SetState(GetTargetState(_currentInput));
    }


    private void Move()
    {
        var strafeDistance = InputAdapter.LeftMove.Read(this) * InputAdapter.MoveModifier.Read(this) * _strafeSpeed;
        _targetInput.FocusLocalPosition = new Coordinate(_targetInput.FocusLocalPosition
                                                         + _targetInput.FocusLocalRotation * Vector3.right * strafeDistance.x
                                                         + _targetInput.FocusLocalRotation * Vector3.forward * strafeDistance.y)
                                        .ClampToTerrain()
                                        .LocalPlanet;
        var localUp = Vector3.Normalize(_targetInput.FocusLocalPosition);
        var localRight = _targetInput.FocusLocalRotation * Vector3.right;
        var localForward = Quaternion.AngleAxis(90, localRight) * localUp;
        _targetInput.FocusLocalRotation = Quaternion.LookRotation(localForward, localUp);
        
        var cameraCoord = new Coordinate(_targetInput.FocusLocalPosition
                                         + Quaternion.AngleAxis(_targetInput.CameraAngle, localRight) 
                                         * -localForward 
                                         * _targetInput.CameraDistance);
        var maxCameraCoord = new Coordinate(_targetInput.FocusLocalPosition
                                            + Quaternion.AngleAxis(MaxAngle, localRight) 
                                            * -localForward 
                                            * Far.Distance);
        cameraCoord.Altitude = math.clamp(cameraCoord.Altitude,
            TerrainAltitude(cameraCoord),
            maxCameraCoord.Altitude);
        _targetInput.CameraAltitude = cameraCoord.Altitude;
        var cameraVector = _targetInput.FocusLocalPosition - cameraCoord.LocalPlanet.ToVector3();
        _targetInput.CameraAngle = Vector3.Angle(cameraVector.normalized, localForward);
        _targetInput.CameraDistance = math.clamp(cameraVector.magnitude, Near.Distance, Far.Distance);
    }
    
    private CameraState GetTargetState(Input input)
    {
        var localUp = Vector3.Normalize(input.FocusLocalPosition);
        var localRight = input.FocusLocalRotation * Vector3.right;
        var localForward = Quaternion.AngleAxis(90, localRight) * localUp;
        var cameraVector = Quaternion.AngleAxis(input.CameraAngle, localRight) * -localForward * input.CameraDistance;
        
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            FocusParent = Planet.Transform,
            FocusLocalPosition = input.FocusLocalPosition,
            FocusLocalRotation = input.FocusLocalRotation,
            CameraParent = Planet.Transform,
            CameraLocalPosition = input.FocusLocalPosition + cameraVector,
            CameraLocalRotation = quaternion.LookRotation(-cameraVector.normalized, localUp),
            FieldOfView = Ease.Log(Near.Fov, Far.Fov, (cameraVector.magnitude - Near.Distance) / (Far.Distance - Near.Distance)),
        };
    }
    
    float TerrainAltitude(Coordinate coord) => math.max(Planet.Data.PlateTectonics.LandHeightMap.Sample(coord).r,
                                                   Planet.Data.Water.WaterMap.Sample(coord).a)
                                               + CurrentState.Camera.nearClipPlane * 1.5f;

}
