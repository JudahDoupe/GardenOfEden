using Assets.GamePlay.Cameras;
using Unity.Mathematics;
using UnityEngine;

public class SatelliteCamera : CameraPerspective
{
    public float LerpSpeed = 5f;
    public float MovementSpeed = 30f;
    public float ZoomSpeed = 60f;
    public float Fov = 30;

    private Coordinate _coord;

    public override CameraState TransitionToState() => GetTargetState(false);

    private void LateUpdate()
    {
        if (!IsActive) return;

        CameraUtils.SetState(GetTargetState(true));
    }

    public CameraState GetTargetState(bool lerp)
    {
        _coord = IsActive ? _coord : new Coordinate(CurrentState.Camera.position, Planet.LocalToWorld);
        var cameraPosition = CurrentState.Camera.localPosition;
        var translation = IsActive 
            ? new Vector3(Input.GetAxis("Horizontal") * MovementSpeed * Time.deltaTime, Input.GetAxis("Vertical") * -MovementSpeed * Time.deltaTime, -Input.mouseScrollDelta.y * ZoomSpeed)
            : Vector3.zero;

        _coord.Altitude = math.clamp(_coord.Altitude + translation.z, MinAltitude + (IsActive ? -10 : 10), MaxAltitude - (IsActive ? -10 : 10));
        _coord.Lat += translation.y;
        _coord.Lon += translation.x;

        cameraPosition = lerp ? Vector3.Lerp(cameraPosition, _coord.LocalPlanet, Time.deltaTime * LerpSpeed) : (Vector3) _coord.LocalPlanet;
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            CameraParent = Planet.Transform,
            CameraLocalPosition = cameraPosition,
            CameraLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FocusParent = Planet.Transform,
            FocusLocalPosition = EnvironmentDataStore.LandHeightMap.Sample(_coord).r * cameraPosition.normalized,
            FocusLocalRotation = Quaternion.LookRotation(-cameraPosition.normalized, Vector3.up),
            FieldOfView = Fov,
            NearClip = 10,
            FarClip = MaxAltitude + Coordinate.PlanetRadius,
        };
    }
}
