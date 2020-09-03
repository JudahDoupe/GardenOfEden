using System;
using System.Linq;
using UnityEngine;

public class EditorCameraVisitor : ICameraVisitor
{
    private const float MinDistance = 1f;
    private const float MaxDistance = 50f;

    private const float ZoomSpeedMultiplier = 0.1f;
    private const float MoveSpeedMultiplier = 2f;
    private const float DriftSpeedMultiplier = 0.05f;
    private const float ZoomDriftSpeedMultiplier = 0.0001f;

    private readonly Transform _camera;
    private Plant _focusedPlant;
    private Vector3 _position;
    private Vector3 _center;
    private float _directionSign;

    public EditorCameraVisitor(Plant plant)
    {
        _camera = Camera.main.transform;
        _focusedPlant = plant;
        _center = CameraUtils.GetPlantBounds(_focusedPlant).center;
        _position = _camera.transform.position - _center;
        PlantMessageBus.PlantDeath.Subscribe(x =>
        {
            if (x == _focusedPlant)
            {
                _focusedPlant = Singleton.PlantSearchService.GetClosestPlants(_center, 2).Last();
            }
        });
    }
    
    public void VisitCamera(CameraController camera)
    {
        _center = Vector3.Lerp(_center, CameraUtils.GetPlantBounds(_focusedPlant).center, Time.deltaTime * MoveSpeedMultiplier / 2);

        if (!TryControl())
        {
            Drift();
        }

        var lerpSpeed = Time.deltaTime * (MoveSpeedMultiplier * 2);

        _camera.position = Vector3.Lerp(_camera.position, _center + _position, lerpSpeed);
        _camera.LookAt(_center);
    }

    private bool TryControl()
    {
        var verticalMovement = Input.GetAxis("Vertical") * (MoveSpeedMultiplier);
        var horizontalMovement = Input.GetAxis("Horizontal") * (MoveSpeedMultiplier);
        var depthMovement = Input.mouseScrollDelta.y * ZoomSpeedMultiplier;

        if (Math.Abs(verticalMovement) < float.Epsilon 
            && Math.Abs(horizontalMovement) < float.Epsilon
            && Math.Abs(depthMovement) < float.Epsilon)
        {
            return false;
        }


        var targetPosition = Quaternion.AngleAxis(-horizontalMovement, Vector3.up) * (Quaternion.AngleAxis(verticalMovement, _camera.transform.right) * _position);

        if (targetPosition.magnitude * (1 - depthMovement) > MinDistance 
            && targetPosition.magnitude * (1 - depthMovement) < MaxDistance)
        {
            targetPosition.Scale(new Vector3(1 - depthMovement, 1 - depthMovement, 1 - depthMovement));
        }

        if (Singleton.LandService.SampleTerrainHeight(targetPosition + _center) < (targetPosition + _center).y 
            && targetPosition.normalized.y < 0.9f)
        {
            _position = targetPosition;
        }

        if (Math.Abs(horizontalMovement) > float.Epsilon)
        {
            _directionSign = Mathf.Sign(horizontalMovement);
        }

        return true;
    }

    private void Drift()
    {
        var targetPosition = Quaternion.AngleAxis(-DriftSpeedMultiplier * _directionSign, Vector3.up) * _position;
        targetPosition.Scale(new Vector3(1 + ZoomDriftSpeedMultiplier, 1 + ZoomDriftSpeedMultiplier, 1 + ZoomDriftSpeedMultiplier));
        var landHeight = Singleton.LandService.SampleTerrainHeight(targetPosition + _center);

        if (landHeight + 1 > (targetPosition + _center).y)
        {
            targetPosition = Quaternion.AngleAxis(DriftSpeedMultiplier, _camera.transform.right) * targetPosition;
        }
        _position = targetPosition;
    }
}
