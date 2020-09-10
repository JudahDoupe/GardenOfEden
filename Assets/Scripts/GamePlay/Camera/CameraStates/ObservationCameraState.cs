using System;
using System.Linq;
using UnityEngine;

public class ObservationCameraState : ICameraState
{
    private const float MinDistance = 1f;
    private const float MaxDistance = 50f;

    private const float MoveSpeedMultiplier = 2f;
    private const float DriftSpeedMultiplier = 0.05f;
    private const float ZoomDriftSpeedMultiplier = 0.0001f;

    private readonly CameraController _controller;
    private readonly Transform _camera;
    private Vector3 _center;
    private Vector3 _offset;
    private float _directionSign = 1;

    public ObservationCameraState(CameraController controller)
    {
        _controller = controller;
        _camera = Camera.main.transform;
    }

    public void UpdateCamera()
    {
        if ((_controller.FocusedPlant == null || Input.GetKeyDown(KeyCode.Q))
            && !FocusOnRandomPlant())
        {
            _controller.CameraState.SetState(new ExplorationCameraState(_controller));
            return;
        }

        _center = Vector3.Lerp(_center, CameraUtils.GetPlantBounds(_controller.FocusedPlant).center, Time.deltaTime * MoveSpeedMultiplier / 2);

        if (!TryControl())
        {
            Drift();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _controller.UiState.SetState(GameObject.FindObjectOfType<PlantEvolutionUi>());
        }

        var lerpSpeed = Time.deltaTime * (MoveSpeedMultiplier * 2);

        _camera.position = Vector3.Lerp(_camera.position, _center + _offset, lerpSpeed);
        _camera.LookAt(_center);
    }

    public void Enable()
    {
        _center = _camera.position;

        if (_controller.FocusedPlant == null && !FocusOnClosestPlant())
        {
            _controller.CameraState.SetState(new ExplorationCameraState(_controller));
            return;
        }

        _center = CameraUtils.GetPlantBounds(_controller.FocusedPlant).center;
        _offset = _camera.transform.position - _center;
    }

    public void Disable() { }

    private bool TryControl()
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

        if (Singleton.LandService.SampleTerrainHeight(targetoffset + _center) < (targetoffset + _center).y
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
        var landHeight = Singleton.LandService.SampleTerrainHeight(targetPosition + _center);

        if (landHeight + 1 > (targetPosition + _center).y)
        {
            targetPosition = Quaternion.AngleAxis(DriftSpeedMultiplier, _camera.transform.right) * targetPosition;
        }
        _offset = targetPosition;
    }

    private bool FocusOnRandomPlant()
    {
        var plants = Singleton.PlantSearchService.GetPlantsWithinRadius(_center, _offset.magnitude);
        if (plants.Any())
        {
            var plantIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, plants.Count()));
            _controller.FocusedPlant = plants.ElementAt(plantIndex);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool FocusOnClosestPlant()
    {
        _controller.FocusedPlant = Singleton.PlantSearchService.GetClosestPlant(_center);
        return _controller.FocusedPlant != null;
    }
}
