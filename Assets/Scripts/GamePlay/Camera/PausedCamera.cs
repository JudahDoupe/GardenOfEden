using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

public class PausedCamera : MonoBehaviour
{
    public float TransitionTime = 1f;
    public bool IsActive { get; private set; }

    private Transform _camera;
    private Transform _focus;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _focus.parent = null;
        _camera.parent = _focus;
        var targetPos = new Vector3(0, 0, Coordinate.PlanetRadius * -2.4f);
        var time = math.sqrt(Vector3.Distance(targetPos, _camera.localPosition)) / 25f * TransitionTime;
        _camera.AnimatePosition(time, targetPos);
        _focus.AnimatePosition(time, _focus.right  * Coordinate.PlanetRadius * -0.75f);
        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        _camera.LookAt(_focus);
    }
}
