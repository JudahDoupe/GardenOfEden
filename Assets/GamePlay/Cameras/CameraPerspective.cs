using UnityEngine;

namespace Assets.GamePlay.Cameras
{
    public class CameraPerspective : MonoBehaviour, IState
    {
        public float MaxAltitude;
        public float MinAltitude;
        public bool IsActive { get; protected set; }
        public virtual void Enable() => IsActive = true;
        public virtual void Disable() => IsActive = false;
        public CameraState CurrentState { get; set; }
        public Transform Camera => CurrentState.Camera;
        public Transform Focus => CurrentState.Focus;
    }
}
