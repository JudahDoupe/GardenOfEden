using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

public class SatelliteCamera : MonoBehaviour
{
    public float TransitionTime = 0.5f;
    public float LerpSpeed = 5f;
    public float MovementSpeed = 30f;
    public float ZoomSpeed = 15f;
    public float MaxAltitude = 2000;
    public float MinAltitude = 1500;
    public float Fov = 30;
    public bool IsActive { get; private set; }

    private Transform _camera;
    private Transform _focus;
    private float _altitude;
    private bool _doneTransitioning;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _camera.parent = null;
        _altitude = math.clamp(_camera.position.magnitude, MinAltitude, MaxAltitude);
        var targetPos = _camera.position.normalized * _altitude;
        var time = math.sqrt(Vector3.Distance(targetPos, _camera.position)) / 25f * TransitionTime;
        _focus.AnimatePosition(time, Vector3.zero);
        _camera.AnimatePosition(time, _camera.position.normalized * _altitude, () =>
        {
            _focus.LookAt(_focus.position - _camera.position);
            _focus.parent = FindObjectOfType<Planet>().transform;
            _camera.parent = _focus;
            _altitude = Vector3.Distance(_focus.position, _camera.position);
            _doneTransitioning = true;
        });
        StartCoroutine(AnimationUtils.AnimateFloat(time, _camera.GetComponent<Camera>().fieldOfView, Fov, x => _camera.GetComponent<Camera>().fieldOfView = x));

        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        _camera.LookAt(_focus, Vector3.up);

        if (!_doneTransitioning) return;

        var poleAlignment = Vector3.Dot(_focus.forward, Vector3.up);
        var x = Input.GetAxis("Horizontal");
        var y = math.clamp(Input.GetAxis("Vertical"), poleAlignment < 0.99f ? -1 : 0, -0.99f < poleAlignment ? 1 : 0);
        var z = -Input.mouseScrollDelta.y * ZoomSpeed;

        _altitude = math.clamp(_altitude + z, MinAltitude, MaxAltitude);
        _focus.Rotate(Vector3.up, -x * MovementSpeed * Time.deltaTime, Space.World);
        _focus.Rotate(Vector3.right, y * MovementSpeed * Time.deltaTime, Space.Self);
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, _camera.localPosition.normalized * _altitude, Time.deltaTime * LerpSpeed);
    }
}
