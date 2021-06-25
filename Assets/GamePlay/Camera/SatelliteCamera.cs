using System;
using System.Linq;
using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

public class SatelliteCamera : MonoBehaviour
{
    public float LerpSpeed = 5f;
    public float MovementSpeed = 30f;
    public float ZoomSpeed = 60f;
    public float MaxAltitude = 4000;
    public float MinAltitude = 3000;
    public float Fov = 30;
    public bool IsActive { get; private set; }

    private Transform _camera;
    private Transform _focus;
    private float _altitude;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _altitude = math.clamp(_camera.position.magnitude, MinAltitude, MaxAltitude);
        _camera.parent = null;

        var cameraPos = _camera.position.normalized * _altitude;
        var cameraRot = Quaternion.LookRotation(-cameraPos, Vector3.up);
        var focusPos = Vector3.zero;
        var time = new[]
        {
            CameraUtils.GetTransitionTime(_camera.position, cameraPos, 2),
            CameraUtils.GetTransitionTime(_camera.rotation, cameraRot, 1.5f),
        }.Max();
        
        _focus.AnimatePosition(time, focusPos);
        _focus.AnimateRotation(time, cameraRot);
        _camera.GetComponent<Camera>().AnimateFov(time, Fov);
        _camera.AnimateRotation(time, cameraRot);
        _camera.AnimatePosition(time, cameraPos, () =>
        {
            _focus.LookAt(_focus.position - _camera.position);
            _focus.parent = FindObjectOfType<Planet>().transform;
            _camera.parent = _focus;
            _altitude = Vector3.Distance(_focus.position, _camera.position);
            IsActive = true;
        });

    }

    public void Disable()
    {
        IsActive = false;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        _camera.LookAt(_focus, Vector3.up);

        var poleAlignment = Vector3.Dot(_focus.forward, Vector3.up);
        var x = Input.GetAxis("Horizontal");
        var y = math.clamp(Input.GetAxis("Vertical"), poleAlignment < 0.99f ? -1 : 0, -0.99f < poleAlignment ? 1 : 0);
        var z = -Input.mouseScrollDelta.y * ZoomSpeed;
        _altitude = math.min(_altitude + z, MaxAltitude);

        _focus.Rotate(Vector3.up, -x * MovementSpeed * Time.deltaTime, Space.World);
        _focus.Rotate(Vector3.right, y * MovementSpeed * Time.deltaTime, Space.Self);
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, _camera.localPosition.normalized * _altitude, Time.deltaTime * LerpSpeed);

        if (_camera.localPosition.magnitude < MinAltitude)
        {
            Singleton.PerspectiveController.ZoomIn();
        }
        if (_camera.localPosition.magnitude > MaxAltitude)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
    }
}