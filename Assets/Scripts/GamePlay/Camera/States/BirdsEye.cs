using UnityEngine;

namespace CameraState
{
    public class BirdsEye : ICameraState
    {
        public void TransitionTo()
        {
            DI.CameraController.MoveSpeed = 1f;
            DI.CameraController.LookSpeed = 10;
            DI.CameraController.TargetPosition = DI.CameraController.PrimaryFocus.GetPosition() + new Vector3(0, 200, -25);
            DI.CameraController.TargetFocusPosition = DI.CameraController.PrimaryFocus.GetPosition();
        }

        public void TransitionAway() { }

        public void Update() { }
    }
}
