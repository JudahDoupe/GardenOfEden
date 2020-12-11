using System;
using System.Linq;
using System.Timers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlanetaryCamera : MonoBehaviour, ICameraState
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
    private float3 _offset;
    private Coordinate _center;
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

        if (Input.GetKeyDown(KeyCode.M))
        {
            var height = _camera.position.y - Singleton.LandService.SampleTerrainHeight(_controller.FocusPoint);  //Flat World Only
            Singleton.LandService.PullMountain(_controller.FocusPoint, height);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Singleton.LandService.AddSpring(_controller.FocusPoint);
        }

        var newCoord = Vector3.Lerp(_controller.FocusPoint.xyz, _center.xyz, lerpSpeed);
        _controller.FocusPoint = Singleton.LandService.ClampToTerrain(newCoord);
        _camera.position = Vector3.Lerp(_camera.position, _controller.FocusPoint.xyz + _offset, lerpSpeed);
        _camera.LookAt(_controller.FocusPoint.xyz);
        _controller.PostProccessing.GetSetting<DepthOfField>().focusDistance.value = Vector3.Distance(_camera.transform.position, _controller.FocusPoint.xyz);
    }

    public void Enable()
    {
        _offset = _camera.transform.position.ToFloat3() - _controller.FocusPoint.xyz;
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

        var targetoffset = Quaternion.AngleAxis(-horizontalMovement, Vector3.up) * (Quaternion.AngleAxis(verticalMovement, _camera.transform.right) * _offset);

        if (targetoffset.magnitude * (1 - depthMovement) > MinDistance
            && targetoffset.magnitude * (1 - depthMovement) < MaxDistance)
        {
            targetoffset.Scale(new Vector3(1 - depthMovement, 1 - depthMovement, 1 - depthMovement));
        }

        Coordinate target = _controller.FocusPoint.xyz + targetoffset.ToFloat3();
        if (Singleton.LandService.SampleTerrainHeight(target) < target.Altitude
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

    private Vector3? lastMousePos;
    private bool TryMove()
    {
        if (!Input.GetMouseButton(0) || LockMovement)
        {
            lastMousePos = null;
            return false;
        }

        var mousePos = Input.mousePosition;
        mousePos.z = _offset.ToVector3().magnitude;
        var currentMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        if (lastMousePos.HasValue)
        {
            var movementVector = Quaternion.FromToRotation(_camera.forward, Vector3.down) * (lastMousePos.Value - currentMousePos);
            _center.xyz += movementVector.ToFloat3() * MoveSpeedMultiplier;
            _center = Singleton.LandService.ClampToTerrain(_center);
        }

        lastMousePos = currentMousePos;
        return true;
    }
}
