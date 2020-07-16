using System;
using UnityEngine;

public class SpeciesCameraVisitor : ICameraVisitor
{
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    private Vector3 _center;
    private float _radius;

    private readonly Transform _camera;
    
    public SpeciesCameraVisitor()
    {
        _camera = Camera.main.transform;
        _targetPosition = _camera.position;
        var height = _targetPosition.y - DI.LandService.SampleTerrainHeight(_targetPosition);
        _radius = 4 * height;
        _center = _targetPosition + _camera.forward * _radius;
    }
    
    public void VisitCamera(CameraController camera)
    {
        var moving = Move();
        var t = Time.deltaTime * camera.Speed;
        var bounds = GetSpeciesBounds();
        var angle = moving ? 0 : t * 10;

        if (!moving) 
        {
            _center = Vector3.Lerp(_center, bounds.center, t);
        }

        _targetPosition = CameraUtils.RotateAroundBounds(_targetPosition, bounds, angle);
        _targetPosition = CameraUtils.ClampAboveGround(_targetPosition, 5);
        _targetRotation =  CameraUtils.LookAtBoundsCenter(bounds);

        camera.transform.position = Vector3.Lerp(camera.transform.position, _targetPosition, t);
        camera.transform.position = CameraUtils.ClampAboveGround(_camera.position, 5);
        camera.transform.rotation = Quaternion.Slerp(_camera.rotation, _targetRotation, t);

        Debug.DrawLine(_center, _center + new Vector3(_radius,0,0));
        Debug.DrawLine(_center, _center + new Vector3(-_radius,0,0));
        Debug.DrawLine(_center, _center + new Vector3(0,0,_radius));
        Debug.DrawLine(_center, _center + new Vector3(0,0,-_radius));
    }

    private bool Move()
    {
        var verticalMovement = Input.GetAxis("Vertical");
        var horizontalMovement = Input.GetAxis("Horizontal");

        _radius -= Input.mouseScrollDelta.y;
        _radius = Mathf.Clamp(_radius, 1, 75);

        if (Math.Abs(verticalMovement) < float.Epsilon && Math.Abs(horizontalMovement) < float.Epsilon) 
            return false;
        
        var transformation = (_camera.forward * verticalMovement) + (_camera.right * horizontalMovement);
        transformation *= _radius * Time.deltaTime;
        var heightOffset = _center.y - DI.LandService.SampleTerrainHeight(_center);
        _center += transformation;
        _center.y = DI.LandService.SampleTerrainHeight(_center) + heightOffset;
        return true;
    }

    private Bounds GetSpeciesBounds()
    {
        var bounds = new Bounds(_center, new Vector3(0.5f,0.5f,0.5f));
        var plants = GameObject.FindObjectsOfType<Plant>();
        foreach (var plant in plants)
        {
            if (Vector3.Distance(_center, plant.transform.position) < _radius)
            {
                bounds.Encapsulate(plant.transform.position);
            }
        }

        return bounds;
    }
}
