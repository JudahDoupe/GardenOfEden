using System;
using System.Linq;
using UnityEngine;

public class ExplorationCameraState : ICameraState
{
    private const float MaxDistance = 50f;
    private const float MinDistance = 1f;
    private const float SearchSpeedMultiplier = 0.05f;
    private const float MoveSpeedMultiplier = 0.5f;
    
    private readonly CameraController _controller;
    private readonly Transform _camera;
    private Vector3 _center;
    private Vector3 _offset;


    public ExplorationCameraState(CameraController controller)
    {
        _camera = Camera.main.transform;
        _controller = controller;
    }

    public void Enable()
    {
        if (_controller.FocusedPlant == null)
        {
            _center = Singleton.LandService.ClampAboveTerrain(_camera.transform.position + (_camera.transform.forward * 10) + (Vector3.down * 100));
        }
        else
        {
            _center = CameraUtils.GetPlantBounds(_controller.FocusedPlant).center;
        }

        _offset = _camera.transform.position - _center;
    }

    public void Disable() { }

    public void UpdateCamera()
    {
        if (TryMove())
        {
            _controller.FocusedPlant = Singleton.PlantSearchService.GetPlantsWithinRadius(_center, _offset.magnitude).Closest(_center);
        }
        
        var lerpSpeed = Time.deltaTime * (MoveSpeedMultiplier * 2);

        if (_controller.FocusedPlant != null)
        {
            var bounds = CameraUtils.GetPlantBounds(_controller.FocusedPlant);
            _center = Vector3.Lerp(_center, bounds.center, lerpSpeed);
        }

        _camera.position = Vector3.Lerp(_camera.position, _center + _offset, lerpSpeed);
        _camera.LookAt(_center);
    }

    private bool TryMove()
    {
        var timeFactor = Time.deltaTime * 30;
        var verticalMovement = Input.GetAxis("Vertical") * MoveSpeedMultiplier;
        var horizontalMovement = Input.GetAxis("Horizontal") * MoveSpeedMultiplier;
        var depthMovement = Input.mouseScrollDelta.y * MoveSpeedMultiplier / 10 * timeFactor;

        if (Math.Abs(verticalMovement) < float.Epsilon 
            && Math.Abs(horizontalMovement) < float.Epsilon
            && Math.Abs(depthMovement) < float.Epsilon)
        {
            return false;
        }

        var movementVector = _camera.forward * verticalMovement + _camera.right * horizontalMovement;
        movementVector *= _offset.magnitude * SearchSpeedMultiplier;
        _center += movementVector;
        _center = CameraUtils.ClampAboveGround(_center);

        if (_offset.magnitude * (1 - depthMovement) > MinDistance
            && _offset.magnitude * (1 - depthMovement) < MaxDistance)
        {
            _offset.Scale(new Vector3(1 - depthMovement, 1 - depthMovement, 1 - depthMovement));
        }

        _offset = (_camera.position - _center).normalized * _offset.magnitude;

        return true;
    }

}
