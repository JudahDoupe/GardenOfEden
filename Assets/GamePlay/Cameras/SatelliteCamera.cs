using System;
using System.Linq;
using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

public class SatelliteCamera : MonoBehaviour
{
    public float LerpSpeed = 5f;
    public float MovementSpeed = 30f;
    public float ZoomSpeed = 60f;
    public float MaxAltitude = 4000;
    public float MinAltitude = 3000;
    public float Fov = 30;
    public bool IsActive { get; private set; }

    private CameraState _currentState;
    private Coordinate _coord;

    public void Enable(CameraState currentState)
    {
        _currentState = currentState;
        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        _currentState = GetTargetState(_currentState, true);
        CameraUtils.SetState(_currentState);

        if (_currentState.Camera.localPosition.magnitude < MinAltitude)
        {
            Singleton.PerspectiveController.ZoomIn();
        }
        if (_currentState.Camera.localPosition.magnitude > MaxAltitude)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
    }

    public CameraState GetTargetState(CameraState currentState, bool lerp)
    {
        _coord = IsActive ? _coord : new Coordinate(currentState.Camera.position, Planet.LocalToWorld);
        var cameraPosition = (Vector3) _coord.LocalPlanet;
        var poleAlignment = Vector3.Dot(currentState.Camera.forward, Vector3.up);
        var translation = IsActive 
            ? new Vector3(Input.GetAxis("Horizontal") * MovementSpeed, Input.GetAxis("Vertical") * -MovementSpeed, -Input.mouseScrollDelta.y * ZoomSpeed)
            : Vector3.zero;

        _coord.Altitude = math.clamp(_coord.Altitude + translation.z, MinAltitude + (IsActive ? -1 : 1), MaxAltitude - (IsActive ? -1 : 1));
        _coord.Lat += translation.y;
        _coord.Lon += translation.x;

        cameraPosition = lerp ? Vector3.Lerp(cameraPosition, _coord.LocalPlanet, Time.deltaTime * LerpSpeed) : (Vector3 ) _coord.LocalPlanet;
        return new CameraState(currentState.Camera, currentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = cameraPosition,
            CameraLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FocusParent = Planet.Transform,
            FocusLocalPosition = Singleton.Land.SampleHeight(_coord) * cameraPosition.normalized,
            FocusLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
        };
    }
}
