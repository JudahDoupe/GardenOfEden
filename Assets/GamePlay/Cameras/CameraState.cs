using UnityEngine;

public struct CameraState
{
    public CameraState(Transform camera, Transform focus)
    {
        Camera = camera;
        CameraParent = camera.parent;
        CameraLocalPosition = camera.localPosition;
        CameraLocalRotation = camera.localRotation;
        Focus = focus;
        FocusParent = focus.parent;
        FocusLocalPosition = focus.localPosition;
        FocusLocalRotation = focus.localRotation;
        FieldOfView = camera.GetComponent<Camera>().fieldOfView;
        Cursor = CursorLockMode.None;
        NearClip = camera.GetComponent<Camera>().nearClipPlane;
        FarClip = camera.GetComponent<Camera>().farClipPlane;
    }

    public Transform Camera;
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