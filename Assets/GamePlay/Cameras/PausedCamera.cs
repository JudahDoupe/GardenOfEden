using Assets.GamePlay.Cameras;
using UnityEngine;

public class PausedCamera : CameraController
{
    public float Fov = 30f;

    public void Enable()
    {
        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }

    public CameraState GetTargetState(CameraState curentState)
    {
        var rotation = Quaternion.LookRotation(curentState.Camera.forward, Vector3.up);
        var pos = rotation * new Vector3(Coordinate.PlanetRadius * -0.66f, 0, Coordinate.PlanetRadius * -5f);
        return new CameraState(curentState.Camera, curentState.Focus)
        {
            CameraParent = null,
            CameraLocalPosition = pos,
            CameraLocalRotation = rotation,
            FocusParent = null,
            FocusLocalPosition = Vector3.zero,
            FocusLocalRotation = rotation,
            FieldOfView = Fov,
        };
    }
}
