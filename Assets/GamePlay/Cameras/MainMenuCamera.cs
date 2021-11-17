using Assets.GamePlay.Cameras;
using UnityEngine;

public class MainMenuCamera : CameraPerspective
{
    public float Fov = 30f;

    public override CameraState TransitionToState() => GetTargetState();

    public CameraState GetTargetState()
    {
        var rotation = Quaternion.LookRotation(CurrentState.Camera.forward, Vector3.up);
        var pos = rotation * new Vector3(Coordinate.PlanetRadius * -0.66f, 0, Coordinate.PlanetRadius * -5f);
        return new CameraState(CurrentState.Camera, CurrentState.Focus)
        {
            CameraParent = null,
            CameraLocalPosition = pos,
            CameraLocalRotation = rotation,
            FocusParent = null,
            FocusLocalPosition = Vector3.zero,
            FocusLocalRotation = rotation,
            FieldOfView = Fov,
            NearClip = 10,
            FarClip = 10000
        };
    }
}
