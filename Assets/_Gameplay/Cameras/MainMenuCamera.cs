using Assets.GamePlay.Cameras;
using UnityEngine;

public class MainMenuCamera : CameraPerspective
{
    public float Fov = 30f;

    public override CameraState StartTransitionTo()
    {
        var currentState = CameraController.CurrentState;
        var rotation = Quaternion.LookRotation(currentState.Camera.transform.forward, Vector3.up);
        var pos = rotation * new Vector3(Coordinate.PlanetRadius * -0.66f, 0, Coordinate.PlanetRadius * -5f);
        return new CameraState(currentState.Camera, currentState.Focus)
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
