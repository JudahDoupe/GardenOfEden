using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class MovePlateTool : MonoBehaviour
{
    public float MaxVelocity = 10;

    private bool _isActive;
    private int _currentPlateId;
    private Coordinate _currentCoord;

    public void Enable()
    {
        _isActive = true;
    }
    public void Disable()
    {
        _isActive = false;
    }

    void Update()
    {
        if (!_isActive) return;

        UpdateContols();
    }

    private void UpdateContols()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, distance))
            {
                _currentCoord = new Coordinate(hit.point, Planet.LocalToWorld);
                _currentCoord.Altitude = Coordinate.PlanetRadius;
                _currentPlateId = (int) math.round(EnvironmentDataStore.ContinentalIdMap.Sample(_currentCoord).r);
            }
            else {
                _currentPlateId = 0;
            }
        }
        if (Input.GetMouseButton(0) && _currentPlateId > 0)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var targetPos = Physics.Raycast(ray, out var hit, distance) ? hit.point : Camera.main.transform.position + ray.direction * distance;
            var plate = Singleton.PlateTectonics.Plates.Single(x => x.Id == _currentPlateId);

            var targetCoord = new Coordinate(targetPos, Planet.LocalToWorld);
            var motionVector = Vector3.ClampMagnitude(targetCoord.LocalPlanet - _currentCoord.LocalPlanet, MaxVelocity).ToFloat3();
            targetCoord.LocalPlanet = _currentCoord.LocalPlanet + motionVector;

            var lastRotation = Quaternion.LookRotation(_currentCoord.LocalPlanet, Camera.main.transform.up);
            var targetRotation = Quaternion.LookRotation(targetCoord.LocalPlanet, Camera.main.transform.up);
            var targetVelocity = targetRotation * Quaternion.Inverse(lastRotation);

            plate.TargetVelocity = targetVelocity;
            _currentCoord.LocalPlanet = plate.Velocity * _currentCoord.LocalPlanet.ToVector3();
        }
        if (Input.GetMouseButtonUp(0))
        {
            var plate = Singleton.PlateTectonics.Plates.FirstOrDefault(x => x.Id == _currentPlateId);
            if(plate != null)
            {
                plate.TargetVelocity = Quaternion.identity;
            }
            _currentPlateId = 0;
        }
    }
}
