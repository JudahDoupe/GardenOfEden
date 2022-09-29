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
        var target = new Coordinate(CameraController.CurrentState.Camera.transform.position, Planet.LocalToWorld);
        _targetCoord.Lat = math.clamp(_targetCoord.Lat, PoleBuffer, 180 - PoleBuffer);
        _targetCoord.Lat = math.clamp(_targetCoord.Lat, PoleBuffer, 180 - PoleBuffer);
        _targetCoord.Altitude = math.clamp(_targetCoord.Altitude, MinAltitude, MaxAltitude);
        return GetTargetState(target);
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
        InputAdapter.LeftMove.Unubscribe(this);
        InputAdapter.Scroll.Unubscribe(this);
        InputAdapter.Click.Unubscribe(this);
        InputAdapter.Drag.Unubscribe(this);
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
        var t = Ease.Out((MinAltitude - cameraPosition.magnitude) / (MinAltitude - MaxAltitude));
        var height = Planet.Data.PlateTectonics.LandHeightMap.Sample(coord).r;
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

    private void Zoom(float delta)
    {
        var t = Ease.Out((MinAltitude - _targetCoord.Altitude) / (MinAltitude - MaxAltitude));
        var z = math.lerp(Near.ZoomSpeed, Far.ZoomSpeed, t) * Coordinate.PlanetRadius;
        _targetCoord.Altitude = math.clamp(_targetCoord.Altitude + delta * z, MinAltitude, MaxAltitude);
    }

    private void Move(Vector2 delta)
    {
        var t = Ease.Out((MinAltitude - _targetCoord.Altitude) / (MinAltitude - MaxAltitude));
        var m = math.lerp(Near.MovementSpeed, Far.MovementSpeed, t) * Time.deltaTime;
        _targetCoord.Lat = math.clamp(_targetCoord.Lat + (delta.y * -m), PoleBuffer, 180 - PoleBuffer);
        _targetCoord.Lon += delta.x * m;
    }
}
