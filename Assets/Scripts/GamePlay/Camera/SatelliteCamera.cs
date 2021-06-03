using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

public class SatelliteCamera : MonoBehaviour
{
    public float TransitionTime = 0.5f;
    public float MovementSpeed = 0.1f;
    public float MaxAltitude = 2000;
    public float MinAltitude = 1000;
    public bool IsActive { get; private set; }

    private Transform _camera;
    private Transform _focus;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _camera.parent = null;
        var targetPos = new Vector3(0, 0, Coordinate.PlanetRadius * -2.4f);
        var time = math.sqrt(Vector3.Distance(targetPos, _camera.localPosition)) / 25f * TransitionTime;
        _focus.AnimatePosition(time, Vector3.zero);
        _camera.AnimatePosition(time, _camera.position.normalized * math.clamp(_camera.position.magnitude, MinAltitude, MaxAltitude), () =>
        {
            _focus.LookAt(_focus.position - _camera.position);
            _camera.parent = _focus;
        });

        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");

        _focus.Rotate(Vector3.up, -x * MovementSpeed * Time.deltaTime, Space.World);

        y = math.clamp(y, 
            Vector3.Dot(_focus.forward, Vector3.up) > -0.99f ? -1 : 0,
            Vector3.Dot(_focus.forward, Vector3.up) < 0.99f ? 1 : 0);
        _focus.Rotate(Vector3.right, y * MovementSpeed * Time.deltaTime, Space.Self);
        _camera.LookAt(_focus, Vector3.up);
    }
}
