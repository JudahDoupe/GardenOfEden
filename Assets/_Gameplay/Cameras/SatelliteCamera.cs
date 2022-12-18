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
    public Texture2D CursorTexture;
    
    [SerializeField]
    private Settings Near; 
    [SerializeField]
    private Settings Far; 

    private Coordinate _targetCoord;
    private bool _isDragging;

    public override CameraState StartTransitionTo()
    {
        _targetCoord = new Coordinate(CameraController.CurrentState.Camera.transform.position, Planet.LocalToWorld);
        _targetCoord.Lat = math.clamp(_targetCoord.Lat, PoleBuffer, 180 - PoleBuffer);
        _targetCoord.Lat = math.clamp(_targetCoord.Lat, PoleBuffer, 180 - PoleBuffer);
        _targetCoord.Altitude = math.clamp(_targetCoord.Altitude, MinAltitude, MaxAltitude);
        return GetTargetState(_targetCoord);
    }

    public override void Enable()
    {
        InputAdapter.LeftMove.Subscribe(this);
        InputAdapter.Scroll.Subscribe(this, callback: Zoom);
        InputAdapter.Click.Subscribe(this,
            startCallback: () => _isDragging = true,
            finishCallback: () => _isDragging = false,
            priority: InputPriority.Low);
        InputAdapter.Drag.Subscribe(this, 
            callback: delta =>
            {
                if (_isDragging) 
                    Move(-DragSpeed * delta); 
            },
            priority: InputPriority.Low);
        Cursor.SetCursor(CursorTexture, new Vector2(CursorTexture.width / 2f, CursorTexture.height / 2f), CursorMode.Auto);

        _targetCoord = new Coordinate(CameraController.CurrentState.Camera.transform.position, Planet.LocalToWorld);
        IsActive = true;
    }
    public override void Disable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        InputAdapter.LeftMove.Unsubscribe(this);
        InputAdapter.Scroll.Unsubscribe(this);
        InputAdapter.Click.Unsubscribe(this);
        InputAdapter.Drag.Unsubscribe(this);
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        Move(InputAdapter.LeftMove.Read(this));
        CameraUtils.SetState(GetTargetState(_targetCoord, true));
    }

    private CameraState GetTargetState(Coordinate coord, bool lerp = false)
    {
        var currentState = CameraController.CurrentState;
        var cameraPosition = lerp 
            ? Vector3.Lerp(currentState.Camera.transform.localPosition, coord.LocalPlanet, Time.deltaTime * LerpSpeed) 
            : coord.LocalPlanet.ToVector3();
        var height = Planet.Data.PlateTectonics.LandHeightMap.Sample(coord).r;
        return new CameraState(currentState.Camera, currentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = cameraPosition,
            CameraLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FocusParent = Planet.Transform,
            FocusLocalPosition = height * cameraPosition.normalized,
            FocusLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FieldOfView = Setting(Near.Fov, Far.Fov, cameraPosition.magnitude),
            NearClip = 10,
            FarClip = MaxAltitude + Coordinate.PlanetRadius,
        };
    }

    private void Zoom(float delta)
    {
        var zoomSpeed = Setting(Near.ZoomSpeed, Far.ZoomSpeed, _targetCoord.Altitude);
        var zoomDistance = delta * zoomSpeed;
        _targetCoord.Altitude = math.clamp(_targetCoord.Altitude + zoomDistance, MinAltitude, MaxAltitude);
    }

    private void Move(Vector2 delta)
    {
        var movementSpeed = Setting(Near.MovementSpeed, Far.MovementSpeed, _targetCoord.Altitude);
        var movementDistance = new Vector2(delta.x, -delta.y) * movementSpeed;
        _targetCoord.Lat = math.clamp(_targetCoord.Lat + movementDistance.y, PoleBuffer, 180 - PoleBuffer);
        _targetCoord.Lon += movementDistance.x;
    }

    private float Setting(float min, float max, float altitude) => Ease.Log(min, max, (altitude - MinAltitude) / (MaxAltitude - MinAltitude));
}
