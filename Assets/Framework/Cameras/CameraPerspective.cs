using UnityEngine;

namespace Assets.GamePlay.Cameras
{
    public class CameraPerspective : MonoBehaviour, IState
    {
        public float MaxAltitude;
        public float MinAltitude;
        public bool IsActive { get; protected set; } = false;

        public virtual CameraState CurrentState => new(CameraController.Instance.Camera, CameraController.Instance.Focus);
        public virtual CameraState StartTransitionTo() => CurrentState;
        public virtual void Enable() => IsActive = true;
        public virtual void Disable() => IsActive = false;
    }

    public struct CameraTransition
    {
        public CameraTransition(float speed, EaseType ease)
        {
            Speed = speed;
            Ease = ease;
        }

        public float Speed;
        public EaseType Ease;
        public static CameraTransition Instant => new CameraTransition(0, EaseType.Linear);
        public static CameraTransition Smooth => new CameraTransition(1, EaseType.InOut);
        public static CameraTransition SmoothFast => new CameraTransition(2f, EaseType.InOut);
    }

    public struct CameraState
    {
        public CameraState(Camera camera, Transform focus)
        {
            Camera = camera;
            var cameraTransform = camera.transform;
            CameraParent = cameraTransform.parent;
            CameraLocalPosition = cameraTransform.localPosition;
            CameraLocalRotation = cameraTransform.localRotation;
            Focus = focus;
            FocusParent = focus.parent;
            FocusLocalPosition = focus.localPosition;
            FocusLocalRotation = focus.localRotation;
            FieldOfView = camera.fieldOfView;
            Cursor = CursorLockMode.None;
            NearClip = camera.nearClipPlane;
            FarClip = camera.farClipPlane;
        }

        public Camera Camera;
        public Transform CameraParent;
        public Vector3 CameraLocalPosition;
        public Quaternion CameraLocalRotation;
        public Transform Focus;
        public Transform FocusParent;
        public Vector3 FocusLocalPosition;
        public Quaternion FocusLocalRotation;
        public float FieldOfView;
        public CursorLockMode Cursor;
        public float NearClip;
        public float FarClip;
    }
}
