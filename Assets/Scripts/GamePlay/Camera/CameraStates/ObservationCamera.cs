using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ObservationCamera : MonoBehaviour, ICameraState
{
    public float MinDistance = 1f;
    public float MaxDistance = 50f;
    public float MoveSpeedMultiplier = 2f;
    public float DriftSpeedMultiplier = 0.05f;
    public float ZoomDriftSpeedMultiplier = 0.0001f;

    private CameraController _controller;
    private Transform _camera;
    private Vector3 _offset;
    private float _directionSign = 1;

    private void Start()
    {
        _camera = Camera.main.transform;
        _controller = FindObjectOfType<CameraController>();
    }

    public void UpdateCamera()
    {
        var lerpSpeed = Time.deltaTime * MoveSpeedMultiplier * 2;

        if (!Move())
        {
            Drift();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _controller.CameraState.SetState(FindObjectOfType<ExplorationCamera>());
        }

        _camera.position = Vector3.Lerp(_camera.position, _controller.FocusPoint + _offset, lerpSpeed);
        _camera.LookAt(_controller.FocusPoint);
        _controller.PostProccessing.GetSetting<DepthOfField>().focusDistance.value = Vector3.Distance(_camera.transform.position, _controller.FocusPoint);
    }

    public void Enable()
    {
        _offset = _camera.transform.position - _controller.FocusPoint;
    }

    public void Disable() { }

    private bool Move()
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

    private void Drift()
    {
        var targetPosition = Quaternion.AngleAxis(-DriftSpeedMultiplier * _directionSign, Vector3.up) * _offset;
        targetPosition.Scale(new Vector3(1 + ZoomDriftSpeedMultiplier, 1 + ZoomDriftSpeedMultiplier, 1 + ZoomDriftSpeedMultiplier));
        var landHeight = Singleton.LandService.SampleTerrainHeight(targetPosition + _controller.FocusPoint);

        if (landHeight + 1 > (targetPosition + _controller.FocusPoint).y)
        {
            targetPosition = Quaternion.AngleAxis(DriftSpeedMultiplier, _camera.transform.right) * targetPosition;
        }
        _offset = targetPosition;
    }
}
