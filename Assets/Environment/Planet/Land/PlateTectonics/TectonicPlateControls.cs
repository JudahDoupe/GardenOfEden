using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TectonicPlateControls : MonoBehaviour
{
    public float MaxVelocity = 10;
    [Range(0,1)]
    public float VelocityLerp = 0.5f;

    private bool _isActive;
    private int _currentPlateId;
    private Coordinate _lastCoord;

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
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                _lastCoord = new Coordinate(hit.point, Planet.LocalToWorld);
                _currentPlateId = (int) math.round(EnvironmentDataStore.ContinentalIdMap.Sample(_lastCoord).r);
            }
            else {
                _currentPlateId = 0;
            }
        }
        if (Input.GetMouseButton(0) && _currentPlateId > 0)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var currentCoord = new Coordinate(hit.point, Planet.LocalToWorld);
                var target = Vector3.Lerp(_lastCoord.LocalPlanet, currentCoord.LocalPlanet, VelocityLerp);
                var vector = target - _lastCoord.LocalPlanet.ToVector3();
                var velocity = vector.normalized * math.min(vector.magnitude, MaxVelocity);
                var plate = Singleton.PlateTectonics.Plates.Single(x => x.Id == _currentPlateId);
                plate.Velocity = Vector3.Lerp(plate.Velocity, velocity, VelocityLerp);

                _lastCoord.LocalPlanet += plate.Velocity.ToFloat3();
                _lastCoord.Altitude = Coordinate.PlanetRadius;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            _currentPlateId = 0;
        }
    }
}
