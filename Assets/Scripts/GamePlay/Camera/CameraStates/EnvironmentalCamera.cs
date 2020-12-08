using System;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EnvironmentalCamera : MonoBehaviour, ICameraState
{
    public static bool LockMovement;
    public static bool LockRotation;
    public static bool LockDrift;

    public float MinDistance = 1f;
    public float MaxDistance = 50f;
    public float MoveSpeedMultiplier = 2f;
    public float DriftSpeedMultiplier = 0.05f;
    public float ZoomDriftSpeedMultiplier = 0.0001f;

    private CameraController _controller;
    private Transform _camera;
    private Vector3 _offset;
    private Vector3 _center;
    private float _directionSign = 1;

    private void Start()
    {
        _camera = Camera.main.transform;
        _controller = FindObjectOfType<CameraController>();
        _center = _controller.FocusPoint;
    }

    public void UpdateCamera()
    {
        var lerpSpeed = Time.deltaTime * MoveSpeedMultiplier * 2;
        
        TryRotate();
        TryMove();
        TryDrift();

        if (Input.GetKeyDown(KeyCode.M))
        {
            var height = _camera.position.y - Singleton.LandService.SampleTerrainHeight(_controller.FocusPoint);  //Flat World Only
            Singleton.LandService.PullMountain(_controller.FocusPoint, height);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Singleton.LandService.AddSpring(_controller.FocusPoint);
        }

        _controller.FocusPoint = Singleton.LandService.ClampToTerrain(Vector3.Lerp(_controller.FocusPoint, _center, lerpSpeed));
        _camera.position = Vector3.Lerp(_camera.position, _controller.FocusPoint + _offset, lerpSpeed);
        _camera.LookAt(_controller.FocusPoint);
        _controller.PostProccessing.GetSetting<DepthOfField>().focusDistance.value = Vector3.Distance(_camera.transform.position, _controller.FocusPoint);
    }

    public void Enable()
    {
        _offset = _camera.transform.position - _controller.FocusPoint;
        _center = _controller.FocusPoint;
    }

    public void Disable() { }

    private bool TryRotate()
    {
        var timeFactor = Time.deltaTime * 30;
        var verticalMovement = Input.GetAxis("Vertical") * (MoveSpeedMultiplier) * timeFactor;
        var horizontalMovement = Input.GetAxis("Horizontal") * (MoveSpeedMultiplier) * timeFactor;
        var depthMovement = Input.mouseScrollDelta.y * MoveSpeedMultiplier / 10 * timeFactor;

        if (Math.Abs(verticalMovement) < float.Epsilon
            && Math.Abs(horizontalMovement) < float.Epsilon
            && Math.Abs(depthMovement) < float.Epsilon)
        {
            return false;
        }

        driftTimeout = 5;
        var targetoffset = Quaternion.AngleAxis(-horizontalMovement, Vector3.up) * (Quaternion.AngleAxis(verticalMovement, _camera.transform.right) * _offset);

        if (targetoffset.magnitude * (1 - depthMovement) > MinDistance
            && targetoffset.magnitude * (1 - depthMovement) < MaxDistance)
        {
            targetoffset.Scale(new Vector3(1 - depthMovement, 1 - depthMovement, 1 - depthMovement));
        }

        if (Singleton.LandService.SampleTerrainHeight(targetoffset + _controller.FocusPoint) < (targetoffset + _controller.FocusPoint).y
            && targetoffset.normalized.y < 0.9f)
        {
            _offset = targetoffset;
        }

        if (Math.Abs(horizontalMovement) > float.Epsilon)
        {
            _directionSign = Mathf.Sign(horizontalMovement);
        }

        return true;
    }

    private float driftTimeout = 5;
    private void TryDrift()
    {
        driftTimeout -= Time.deltaTime;
        if (driftTimeout > 0) return;

        var driftSpeed = Mathf.Min(DriftSpeedMultiplier, Mathf.Abs(driftTimeout) / 250);
        var zoomSpeed = 1 + Mathf.Min(ZoomDriftSpeedMultiplier, Mathf.Abs(driftTimeout) / 250);

        var targetPosition = (Quaternion.AngleAxis(-driftSpeed * _directionSign, Vector3.up) * _offset) * zoomSpeed;
        var landHeight = Singleton.LandService.SampleTerrainHeight(targetPosition + _controller.FocusPoint);

        if (landHeight + 1 > (targetPosition + _controller.FocusPoint).y)
        {
            targetPosition = Quaternion.AngleAxis(driftSpeed, _camera.transform.right) * targetPosition;
        }
        _offset = targetPosition;
    }

    private Vector3? lastMousePos;
    private bool TryMove()
    {
        if (!Input.GetMouseButton(0) || LockMovement)
        {
            lastMousePos = null;
            return false;
        }

        driftTimeout = 5;
        var mousePos = Input.mousePosition;
        mousePos.z = _offset.magnitude;
        var currentMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        if (lastMousePos.HasValue)
        {
            var movementVector = Quaternion.FromToRotation(_camera.forward, Vector3.down) * (lastMousePos.Value - currentMousePos);
            _center += movementVector * MoveSpeedMultiplier;
            _center = Singleton.LandService.ClampToTerrain(_center);
        }

        lastMousePos = currentMousePos;
        return true;
    }
}
