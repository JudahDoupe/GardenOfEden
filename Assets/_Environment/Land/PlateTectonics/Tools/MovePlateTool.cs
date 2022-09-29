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
    private PlateBaker _baker; 
    private float _currentPlateId;
    private Vector3 _startingPosition;

    public void Initialize(PlateTectonicsData data,
        PlateTectonicsSimulation simulation,
        PlateTectonicsVisualization visualization,
        PlateBaker baker)
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

        Clear();
        _simulation.Enable();
        _visualization.ShowFaultLines(true);
        IsActive = true;

        InputAdapter.Click.Subscribe(this,
            startCallback: () =>
            {
                StartMoving();
            },
            finishCallback: () =>
            {
                Clear();
            });
    }
    public void Disable()
    {
        Clear();
        _visualization.ShowFaultLines(false);
        IsActive = false; 
        InputAdapter.Click.Unubscribe(this);
    }

    private void Update()
    {

        if (IsActive && _currentPlateId > 0) Move();
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
            _baker.Disable();
        }
        else
        {
            _currentPlateId = 0;
        }
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
    }

    private void Clear()
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

        if (_simulation.IsActive)
        {
            _baker.Enable();
        }
    }
}
