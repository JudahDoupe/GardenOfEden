using UnityEngine;

public class MovePlateTool : MonoBehaviour, ITool
{
    public float MaxVelocity = 10;

    public bool IsInitialized { get; private set; }
    public bool IsActive { get; private set; }

    private PlateTectonicsData _data;
    private PlateTectonicsVisualization _visualization;
    private PlateTectonicsSimulation _simulation; 
    private float _currentPlateId;
    private Coordinate _startingCoord;

    public void Initialize(PlateTectonicsData data,
        PlateTectonicsSimulation simulation,
        PlateTectonicsVisualization visualization)
    {
        _data = data;
        _simulation = simulation;
        _visualization = visualization;
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
    }
    public void Disable()
    {
        Clear();
        _visualization.ShowFaultLines(false);
        IsActive = false;
    }

    void Update()
    {
        if (!IsActive) return;

        if (Input.GetMouseButtonDown(0)) StartMoving();
        if (Input.GetMouseButton(0) && _currentPlateId > 0) Move();
        if (Input.GetMouseButtonUp(0) && _currentPlateId > 0) Clear();
    }

    private void StartMoving()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, distance))
        {
            _startingCoord = new Coordinate(hit.point, Planet.LocalToWorld)
            {
                Altitude = Coordinate.PlanetRadius
            };
            _currentPlateId = _data.ContinentalIdMap.SamplePoint(_startingCoord).r;
            var plate = _data.GetPlate(_currentPlateId);
            _startingCoord.LocalPlanet = Quaternion.Inverse(plate.Rotation) * _startingCoord.LocalPlanet;
            Debug.Log(_startingCoord.LocalPlanet);
        }
        else
        {
            _currentPlateId = 0;
        }
    }

    private void Move()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var targetPos = Physics.Raycast(ray, out var hit, distance) ? hit.point : Camera.main.transform.position + ray.direction * distance;
        targetPos = new Coordinate(targetPos, Planet.LocalToWorld).LocalPlanet;
        var plate = _data.GetPlate(_currentPlateId);

        var currentPos = plate.Rotation * _startingCoord.LocalPlanet;
        Debug.Log(currentPos);
        var motionVector = Vector3.ClampMagnitude(targetPos - currentPos, MaxVelocity);
        targetPos = currentPos + motionVector;

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
    }
}
