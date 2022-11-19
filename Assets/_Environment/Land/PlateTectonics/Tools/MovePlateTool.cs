using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlateTool : MonoBehaviour, ITool
{
    public float MaxVelocity = 10;
    [Range(0,1)]
    public float Dampening = 0.5f;

    public bool IsInitialized { get; private set; }
    public bool IsActive { get; private set; }

    private PlateTectonicsData _data;
    private PlateTectonicsVisualization _visualization;
    private PlateTectonicsSimulation _simulation; 
    private PlateBakerV2 _baker; 

    private float _currentPlateId;
    private Vector3 _startingPosition;
    private bool _needsBaking = false;

    public void Initialize(PlateTectonicsData data,
        PlateTectonicsSimulation simulation,
        PlateTectonicsVisualization visualization,
        PlateBakerV2 baker)
    {
        _data = data;
        _simulation = simulation;
        _visualization = visualization;
        _baker = baker;
        IsInitialized = true;
    }
    public void Enable()
    {
        if (!IsInitialized)
            return;

        StopMoving();
        _simulation.Enable();
        IsActive = true;

        InputAdapter.Click.Subscribe(this,
            startCallback: () =>
            {
                StartMoving();
            },
            finishCallback: () =>
            {
                StopMoving();
            });
    }
    public void Disable()
    {
        StopMoving();
        InputAdapter.Click.Unubscribe(this);
        _visualization.HideOutlines();
        IsActive = false;
        TryBake();
    }

    private void Update()
    {
        if (IsActive)
        {
            _visualization.OutlinePlates();
            TryMove();

            if (_data.Plates.Any(x => x.IsInMotion))
            {
                _needsBaking = true;
                _baker.CancelBake();
            }
            else
            {
                TryBake();
            }
        }
    }



    private void TryBake()
    {
        if (_needsBaking)
        {
            _needsBaking = false;
            _data.LandHeightMap.RefreshCache();
            _baker.BakePlates();
        }
    }

    private void TryMove()
    {
        if (_currentPlateId > 0)
            Move();
    }

    private void Move()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        var targetPos = Physics.Raycast(ray, out var hit, distance) ? hit.point : Camera.main.transform.position + ray.direction * distance;
        targetPos = new Coordinate(targetPos, Planet.LocalToWorld).LocalPlanet;
        var plate = _data.GetPlate(_currentPlateId);

        var currentPos = plate.Rotation * _startingPosition;
        var motionVector = Vector3.ClampMagnitude(targetPos - currentPos, MaxVelocity);
        var remainingDistance = Vector3.Distance(currentPos, targetPos);
        var totalDistance = Vector3.Distance(_startingPosition, targetPos);
        var speedMultiplier = Mathf.Clamp01(remainingDistance / (totalDistance * Dampening));
        var scaledMotionVector = motionVector * speedMultiplier;
        targetPos = currentPos + scaledMotionVector;

        var lastRotation = Quaternion.LookRotation(currentPos, Camera.main.transform.up);
        var targetRotation = Quaternion.LookRotation(targetPos, Camera.main.transform.up);
        var targetVelocity = targetRotation * Quaternion.Inverse(lastRotation);

        plate.TargetVelocity = targetVelocity;
        _needsBaking = true;
    }

    private void StartMoving()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, distance))
        {
            var coord = new Coordinate(hit.point, Planet.LocalToWorld)
            {
                Altitude = Coordinate.PlanetRadius
            };
            _currentPlateId = _data.ContinentalIdMap.SamplePoint(coord).r;
            var plate = _data.GetPlate(_currentPlateId);
            _startingPosition = Quaternion.Inverse(plate.Rotation) * coord.LocalPlanet;
        }
        else
        {
            _currentPlateId = 0;
        }
    }

    private void StopMoving()
    {
        if (_currentPlateId > 0)
        {
            var plate = _data.GetPlate(_currentPlateId);
            if (plate != null)
            {
                plate.TargetVelocity = Quaternion.identity;
            }
        }
        _currentPlateId = 0;
    }
}
