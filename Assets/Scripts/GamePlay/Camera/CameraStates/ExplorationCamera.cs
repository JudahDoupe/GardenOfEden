using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ExplorationCamera : MonoBehaviour, ICameraState
{
    public float MinDistance = 1f;
    public float MaxDistance = 50f;
    public float MoveSpeedMultiplier = 1f;
    
    private CameraController _controller;
    private Transform _camera;
    private Vector2 _offset;


    private void Start()
    {
        _camera = Camera.main.transform;
        _controller = FindObjectOfType<CameraController>();
    }

    public void Enable()
    {
        _offset = new Vector2(
            Mathf.Abs(_camera.transform.position.x - _controller.FocusPoint.x) + Mathf.Abs(_camera.transform.position.z - _controller.FocusPoint.z),
            _camera.transform.position.y - Singleton.LandService.SampleTerrainHeight(_camera.transform.position));
    }

    public void Disable() { }

    public void UpdateCamera()
    {
        var lerpSpeed = Time.deltaTime * MoveSpeedMultiplier * 2;
        
        _controller.FocusedPlant = Singleton.PlantSearchService
            .GetPlantsWithinRadius(_controller.FocusPoint, _offset.magnitude)
            .Closest(_controller.FocusPoint);

        if (!Move() && _controller.FocusedPlant != null) 
        { 
            _controller.FocusPoint = Vector3.Lerp(_controller.FocusPoint, _controller.FocusedPlant.transform.position, lerpSpeed);

            if (Input.GetKeyDown(KeyCode.F))
            {
                _controller.CameraState.SetState(FindObjectOfType<ObservationCamera>());
            }
        }

        var offsetDirection = (Vector3.Scale(_camera.transform.position, new Vector3(1, 0, 1)) - Vector3.Scale(_controller.FocusPoint, new Vector3(1, 0, 1))).normalized;
        var offset = offsetDirection * _offset.x;
        var newPos = _controller.FocusPoint + offset;
        newPos.y = Singleton.LandService.SampleTerrainHeight(newPos) + _offset.y;

        _camera.position = Vector3.Lerp(_camera.position, newPos, lerpSpeed);
        _camera.LookAt(_controller.FocusPoint);
        _controller.PostProccessing.GetSetting<DepthOfField>().focusDistance.value = Vector3.Distance(_camera.transform.position, _controller.FocusPoint);
    }

    private bool Move()
    {
        var verticalMovement = Input.GetAxis("Vertical") * MoveSpeedMultiplier * Time.deltaTime;
        var horizontalMovement = Input.GetAxis("Horizontal") * MoveSpeedMultiplier * Time.deltaTime;
        var depthMovement = Input.mouseScrollDelta.y * MoveSpeedMultiplier * 2 * Time.deltaTime;

        if (Math.Abs(verticalMovement) < float.Epsilon 
            && Math.Abs(horizontalMovement) < float.Epsilon
            && Math.Abs(depthMovement) < float.Epsilon)
        {
            return false;
        }

        var zoomFactor = _offset.magnitude / 2;
        var movementVector = (_camera.forward * verticalMovement + _camera.right * horizontalMovement) * zoomFactor;
        _controller.FocusPoint += movementVector;
        _controller.FocusPoint = Singleton.LandService.ClampToTerrain(_controller.FocusPoint);

        if (_offset.magnitude * (1 - depthMovement) > MinDistance
            && _offset.magnitude * (1 - depthMovement) < MaxDistance)
        {
            _offset.Scale(new Vector3(1 - depthMovement, 1 - depthMovement, 1 - depthMovement));
        }

        return true;
    }
}
