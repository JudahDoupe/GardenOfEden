using System.Linq;
using Assets.Scripts.Utils;
using UnityEngine;

public class PausedCamera : MonoBehaviour
{
    public float Fov = 30f;
    public bool IsActive { get; private set; }

    private Transform _camera;
    private Transform _focus;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _focus.parent = null;
        _camera.parent = _focus;

        var cameraPos = new Vector3(Coordinate.PlanetRadius * -0.66f, 0, Coordinate.PlanetRadius * -5f);
        var focusPos = Vector3.zero;
        var cameraRot = Quaternion.identity;
        var focusRot = Quaternion.LookRotation(_camera.forward, Vector3.up);
        var time = new[]
        {
            CameraUtils.GetTransitionTime(_camera.localPosition, cameraPos, 3),
            CameraUtils.GetTransitionTime(_camera.localRotation, cameraRot, 1.5f),
        }.Max();

        _camera.GetComponent<Camera>().AnimateFov(time, Fov);
        _camera.AnimatePosition(time, cameraPos);
        _camera.AnimateRotation(time, cameraRot);
        _focus.AnimateRotation(time, focusRot);
        _focus.AnimatePosition(time, focusPos);

        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }
}
