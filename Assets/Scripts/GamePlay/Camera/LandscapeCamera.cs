using System;
using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

public class LandscapeCamera : MonoBehaviour
{
    public float TransitionTime = 0.5f;
    public float LerpSpeed = 5f;
    public float MovementSpeed = 30f;
    public float RotationSpeed = 1f;
    public float ZoomSpeed = 15f;
    public float MaxAltitude = 3000;
    public float MinAltitude = 2000;
    public float Fov = 30;
    public float FocusDistance = 100;
    public bool IsActive { get; private set; }

    private Transform _camera;
    private Transform _focus;
    private float _altitude;
    private Vector3 _cameraPos;
    private bool _doneTransitioning;

    public void Enable(Transform camera, Transform focus)
    {
        _camera = camera;
        _focus = focus;
        _camera.parent = FindObjectOfType<Planet>().transform;
        _focus.parent = FindObjectOfType<Planet>().transform;
        _altitude = math.clamp(_camera.position.magnitude, MinAltitude, MaxAltitude);

        _focus.LookAt(_camera, _camera.up);
        var focusPos = _camera.localPosition + _focus.up * FocusDistance;
        focusPos = focusPos.normalized * MinAltitude;
        var time = math.sqrt(Vector3.Distance(focusPos, _focus.localPosition)) / 25f * TransitionTime;
        _focus.AnimatePosition(time, focusPos, () =>
        {
            _focus.LookAt(focus.position - _camera.position);
            _cameraPos = _camera.localPosition;
            _doneTransitioning = true;
        }); 
        _camera.AnimatePosition(time, _camera.localPosition.normalized * _altitude);
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
        

        if (!_doneTransitioning)
        {
            _camera.LookAt(_focus, _camera.position.normalized);
        }

        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        var z = -Input.mouseScrollDelta.y * ZoomSpeed;

        _altitude = math.clamp(_altitude + z, MinAltitude, MaxAltitude + 10);
        _camera.Rotate(_camera.localPosition.normalized, x * RotationSpeed);
        _cameraPos += _camera.forward * y * MovementSpeed;
        _cameraPos = _cameraPos.normalized * _altitude;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, _cameraPos, Time.deltaTime * LerpSpeed);

        var forward = (_focus.position.normalized - _camera.position.normalized).normalized;
        var focusPos = _camera.position + forward * FocusDistance;
        _focus.position = focusPos.normalized * MinAltitude;
        _camera.LookAt(_focus, _camera.position.normalized);
        /*
        if (_camera.position.magnitude < MinAltitude)
        {
            Singleton.PerspectiveController.ZoomIn();
        }
        if (_camera.position.magnitude > MaxAltitude)
        {
            Singleton.PerspectiveController.ZoomOut();
        }
        */
    }
}
