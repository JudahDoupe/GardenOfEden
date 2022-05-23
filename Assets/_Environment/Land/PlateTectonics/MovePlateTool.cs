using UnityEngine;

public class MovePlateTool : MonoBehaviour, ITool
{
    public float MaxVelocity = 10;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            _simulation = FindObjectOfType<PlateTectonicsSimulation>();
            _simulation.IsActive = value;
            _visualization = FindObjectOfType<PlateTectonicsVisualization>();
            _visualization.ShowFaultLines(value);
            Clear();
        }
    }

    private PlateTectonicsSimulation _simulation;
    private PlateTectonicsVisualization _visualization;
    private float _currentPlateId;
    private Coordinate _currentCoord;

    void Update()
    {
        if (!_isActive) return;
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
            _currentCoord = new Coordinate(hit.point, Planet.LocalToWorld);
            _currentCoord.Altitude = Coordinate.PlanetRadius;
            _currentPlateId = _simulation.Data.ContinentalIdMap.SamplePoint(_currentCoord).r;
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
        var plate = _simulation.GetPlate(_currentPlateId);
        _currentCoord.LocalPlanet = plate.Velocity * _currentCoord.LocalPlanet.ToVector3();

        var targetCoord = new Coordinate(targetPos, Planet.LocalToWorld);
        var motionVector = Vector3.ClampMagnitude(targetCoord.LocalPlanet - _currentCoord.LocalPlanet, MaxVelocity).ToFloat3();
        targetCoord.LocalPlanet = _currentCoord.LocalPlanet + motionVector;

        var lastRotation = Quaternion.LookRotation(_currentCoord.LocalPlanet, Camera.main.transform.up);
        var targetRotation = Quaternion.LookRotation(targetCoord.LocalPlanet, Camera.main.transform.up);
        var targetVelocity = targetRotation * Quaternion.Inverse(lastRotation);

        plate.TargetVelocity = targetVelocity;
    }

    private void Clear()
    {
        if (_currentPlateId > 0)
        {
            var plate = _simulation.GetPlate(_currentPlateId);
            if (plate != null)
            {
                plate.TargetVelocity = Quaternion.identity;
            }
        }
        _currentPlateId = 0;
    }
}
