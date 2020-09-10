using System;
using System.Linq;
using UnityEngine;

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

        if ((_controller.FocusedPlant == null || Input.GetKeyDown(KeyCode.Q))
            && !FocusOnRandomPlant())
        {
            _controller.CameraState.SetState(FindObjectOfType<ExplorationCamera>());
            return;
        }

        _controller.FocusPoint = Vector3.Lerp(_controller.FocusPoint, CameraUtils.GetPlantBounds(_controller.FocusedPlant).center, lerpSpeed);

        if (!Move())
        {
            Drift();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _controller.UiState.SetState(FindObjectOfType<PlantEvolutionUi>());
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            _controller.CameraState.SetState(FindObjectOfType<ExplorationCamera>());
        }

        _camera.position = Vector3.Lerp(_camera.position, _controller.FocusPoint + _offset, lerpSpeed);
        _camera.LookAt(_controller.FocusPoint);
    }

    public void Enable()
    {
        if (_controller.FocusedPlant == null && !FocusOnClosestPlant())
        {
            _controller.CameraState.SetState(FindObjectOfType<ExplorationCamera>());
            return;
        }

        _offset = _camera.transform.position - CameraUtils.GetPlantBounds(_controller.FocusedPlant).center;
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

    private bool FocusOnRandomPlant()
    {
        var plants = Singleton.PlantSearchService.GetPlantsWithinRadius(_controller.FocusPoint, _offset.magnitude);
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
        _controller.FocusedPlant = Singleton.PlantSearchService.GetClosestPlant(_controller.FocusPoint);
        return _controller.FocusedPlant != null;
    }
}
