using UnityEngine;

namespace Assets.GamePlay.Cameras
{
    public class CameraController : MonoBehaviour
    {
        public bool IsActive { get; protected set; }
        public CameraState CurrentState { get; set; }
        public Transform Camera => CurrentState.Camera;
        public Transform Focus => CurrentState.Focus;
    }
}
