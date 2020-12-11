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

    public float MinDistance = 1f;
    public float MaxDistance = 100f;
    public float MoveSpeedMultiplier = 2f;
    public float RotateSpeedMultiplier = 2f;

    private CameraController _controller;
    private Transform _camera;
    private float3 _offset;
    private Coordinate _center;

    private ILandService _land;

    private void Start()
    {
        _camera = Camera.main.transform;
        _controller = FindObjectOfType<CameraController>();
        _center = _controller.FocusPoint;
        _land = Singleton.LandService;
    }

    public void Enable()
    {
        _offset = _camera.transform.position.ToFloat3() - _controller.FocusPoint.xyz;
        _center = _controller.FocusPoint;
    }

    public void Disable() { }

    public void UpdateCamera()
    {
        var lerpSpeed = Time.deltaTime * MoveSpeedMultiplier * 2;
        
        TryRotate();
        TryMove();

        if (Input.GetKeyDown(KeyCode.M))
        {
            var height = new Coordinate(_camera.position).Altitude - _controller.FocusPoint.Altitude;
            Singleton.LandService.PullMountain(_controller.FocusPoint, height);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Singleton.LandService.AddSpring(_controller.FocusPoint);
        }

        var up = _controller.FocusPoint.xyz.ToVector3().normalized;
        var forward = (_controller.FocusPoint.xyz - _land.ClampToTerrain(new Coordinate(_camera.transform.position)).xyz).ToVector3().normalized;
        var sphericalOffset = up * _offset.y + forward * _offset.z;

        _controller.FocusPoint = _land.ClampToTerrain(Vector3.Lerp(_controller.FocusPoint.xyz, _center.xyz, lerpSpeed));
        _camera.position = Vector3.Lerp(_camera.position, _controller.FocusPoint.xyz + sphericalOffset.ToFloat3(), lerpSpeed);
        _camera.LookAt(_controller.FocusPoint.xyz, up);
        _controller.PostProccessing.GetSetting<DepthOfField>().focusDistance.value = Vector3.Distance(_camera.transform.position, _controller.FocusPoint.xyz);
    }

    private void TryRotate()
    {
        if (!Input.GetMouseButton(0) || LockMovement)
        {
            return;
        }

        var timeFactor = Time.deltaTime * 30;
        var verticalMovement = Input.GetAxis("Mouse Y") * (RotateSpeedMultiplier) * timeFactor;
        var horizontalMovement = Input.GetAxis("Mouse X") * (RotateSpeedMultiplier) * timeFactor;
        var depthMovement = Input.mouseScrollDelta.y * MoveSpeedMultiplier / 10 * timeFactor;

        var targetoffset = Quaternion.AngleAxis(-horizontalMovement, _controller.FocusPoint.xyz.ToVector3().normalized) 
            * (Quaternion.AngleAxis(verticalMovement, _camera.transform.right) * _offset);

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
    }

    private void TryMove()
    {
        var timeFactor = Time.deltaTime * 30;
        var verticalMovement = Input.GetAxis("Vertical") * (MoveSpeedMultiplier) * timeFactor;
        var horizontalMovement = Input.GetAxis("Horizontal") * (MoveSpeedMultiplier) * timeFactor;
        var movementVector = _camera.right * horizontalMovement + (_camera.up + _camera.forward).normalized * verticalMovement;
        
        if (movementVector.magnitude > float.Epsilon)
        {
            _center.xyz += movementVector.ToFloat3() * MoveSpeedMultiplier;
            _center = Singleton.LandService.ClampToTerrain(_center);
        }
    }
}
