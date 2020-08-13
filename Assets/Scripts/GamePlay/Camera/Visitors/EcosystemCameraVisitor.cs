using System;
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
    private Plant _focusedPlant
    {
        get => Singleton.UiController.Data.FocusedPlant;
        set => Singleton.UiController.Data.FocusedPlant = value;
    }
    private Vector3 _center;
    private Vector3 _direction;
    private float _distance;
    private Vector3 _targetPostion;
    private Quaternion _targetRotation;

    private UiState _evolutionUi;
    private UiState _infoUi;


    public EcosystemCameraVisitor(Plant focusedPlant)
    {
        _camera = Camera.main.transform;
        _focusedPlant = focusedPlant;
        _center = _focusedPlant.transform.position;
        _direction = (_camera.position - _center).normalized;
        _distance = Vector3.Distance(_camera.position, _center);
        _evolutionUi = GameObject.FindObjectOfType<PlantEvolutionUi>();
        _infoUi = GameObject.FindObjectOfType<BasicInfoUi>();
    }
    
    public void VisitCamera(CameraController camera)
    {
        if (Singleton.UiController.CurrentState != _evolutionUi)
        {
            var moving = Move();
            if (!moving)
            {
                if (_focusedPlant == null)
                {
                    _focusedPlant = Singleton.PlantSearchService.GetClosestPlant(_center);
                }
                if (_focusedPlant != null)
                {
                    var bounds = CameraUtils.GetPlantBounds(_focusedPlant); 
                    _center = bounds.center;
                    _distance = Mathf.Max(CameraUtils.GetDistanceToIncludeBounds(bounds), _distance);
                }
            }
        }

        _distance -= (Input.mouseScrollDelta.y * ZoomSpeedMultiplier * _distance);
        _distance = Mathf.Clamp(_distance, MinDistance, MaxDistance);
        
        var offset = _direction * _distance;
        _targetPostion = _center + offset;
        _targetPostion.y = Singleton.LandService.SampleTerrainHeight(_center + offset) + _distance * 0.5f;

        _targetRotation = Quaternion.LookRotation(_center - _camera.position, Vector3.up);
        
        _camera.position = Vector3.Lerp(_camera.position, _targetPostion, Time.deltaTime * MoveSpeedMultiplier);
        _camera.rotation = Quaternion.Lerp(_camera.rotation, _targetRotation, Time.deltaTime * LookSpeedMultiplier);

        if (Input.GetKeyDown(KeyCode.E))
        {
            Singleton.UiController.SetState(_evolutionUi);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Singleton.UiController.SetState(_infoUi);
        }
    }
    private bool Move()
    {
        var verticalMovement = Input.GetAxis("Vertical");
        var horizontalMovement = Input.GetAxis("Horizontal");

        if (Math.Abs(verticalMovement) < float.Epsilon && Math.Abs(horizontalMovement) < float.Epsilon) 
            return false;

        _focusedPlant = null;
        var movementVector = _camera.forward * verticalMovement + _camera.right * horizontalMovement;
        movementVector *= _distance;
        movementVector *= SearchSpeedMultiplier;
        _center += movementVector;
        _center = CameraUtils.ClampAboveGround(_center);

        return true;
    }
}
