using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeciesCameraVisitor : ICameraVisitor
{
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    
    private Vector3 _center;
    private float _radius;
    
    public SpeciesCameraVisitor()
    {
        _targetPosition = Camera.main.transform.position;
        var height = _targetPosition.y - DI.LandService.SampleTerrainHeight(_targetPosition);
        _radius = 4 * height;
        _center = _targetPosition + Camera.main.transform.forward * _radius;
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
        camera.transform.position = CameraUtils.ClampAboveGround(camera.transform.position, 5);
        camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, _targetRotation, t);

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

        if (verticalMovement != 0 || horizontalMovement != 0) 
        {
            var transformation = (Camera.main.transform.forward * verticalMovement) + (Camera.main.transform.right * horizontalMovement);
            transformation *= _radius * Time.deltaTime;
            var heightOffset = _center.y - DI.LandService.SampleTerrainHeight(_center);
            _center += transformation;
            _center.y = DI.LandService.SampleTerrainHeight(_center) + heightOffset;
            return true;
        }
        return false;
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
