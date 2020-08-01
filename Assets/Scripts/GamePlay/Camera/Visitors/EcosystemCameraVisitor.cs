﻿using System;
using UIState;
using UnityEngine;

public class EcosystemCameraVisitor : ICameraVisitor
{
    private const float MaxDistance = 50f;
    private const float MinDistance = 1f;
    private const float SearchSpeedMultiplier = 0.05f;
    private const float ZoomSpeedMultiplier = 0.1f;
    private const float MoveSpeedMultiplier = 2f;
    private const float LookSpeedMultiplier = 0.5f;
    
    private readonly Transform _camera;
    private Plant _focusedPlant;
    private Vector3 _center;
    private Vector3 _direction;
    private float _distance;
    private Vector3 _targetPostion;
    private Quaternion _targetRotation;

    private PlantInspectionUi _plantUi;
    
    public EcosystemCameraVisitor(Plant focusedPlant)
    {
        _camera = Camera.main.transform;
        //_plantUi = GameObject.FindObjectOfType<PlantInspectionUi>();
        FocusOnPlant(focusedPlant);
        _center = _focusedPlant.transform.position;
        _direction = (_camera.position - _center).normalized;
        _distance = Vector3.Distance(_camera.position, _center);
    }
    
    public void VisitCamera(CameraController camera)
    {
        var moving = Move();
        if (!moving)
        {
            if (_focusedPlant == null)
            {
                FocusOnPlant(DI.PlantSearchService.GetClosestPlant(_center));
            }
            if (_focusedPlant != null)
            {
                var bounds = CameraUtils.GetPlantBounds(_focusedPlant); 
                _center = bounds.center;
                _distance = Mathf.Max(CameraUtils.GetDistanceToIncludeBounds(bounds), _distance);
            }
        }

        _distance -= (Input.mouseScrollDelta.y * ZoomSpeedMultiplier * _distance);
        _distance = Mathf.Clamp(_distance, MinDistance, MaxDistance);
        
        var offset = _direction * _distance;
        _targetPostion = _center + _direction * _distance;
        _targetPostion.y = DI.LandService.SampleTerrainHeight(_center + _direction * _distance) + _distance * 0.5f;

        _targetRotation = Quaternion.LookRotation(_center - _camera.position, Vector3.up);
        
        _camera.position = Vector3.Lerp(_camera.position, _targetPostion, Time.deltaTime * MoveSpeedMultiplier);
        _camera.rotation = Quaternion.Lerp(_camera.rotation, _targetRotation, Time.deltaTime * LookSpeedMultiplier);
    }
    private bool Move()
    {
        /*
        if (_plantUi.IsActive) 
            return false;
        */
        
        var verticalMovement = Input.GetAxis("Vertical");
        var horizontalMovement = Input.GetAxis("Horizontal");

        if (Math.Abs(verticalMovement) < float.Epsilon && Math.Abs(horizontalMovement) < float.Epsilon) 
            return false;
        
        FocusOnPlant(null);
        var movementVector = _camera.forward * verticalMovement + _camera.right * horizontalMovement;
        movementVector *= _distance;
        movementVector *= SearchSpeedMultiplier;
        _center += movementVector;
        _center = CameraUtils.ClampAboveGround(_center);

        return true;
    }

    private void FocusOnPlant(Plant plant)
    {
        //_plantUi.InspectPlant(plant);
        _focusedPlant = plant;
    }
}
