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
                var plate = Singleton.PlateTectonics.Plates.Single(x => x.Id == _currentPlateId);

                var currentCoord = new Coordinate(hit.point, Planet.LocalToWorld);
                currentCoord.LocalPlanet = _lastCoord.LocalPlanet + Vector3.ClampMagnitude(currentCoord.LocalPlanet - _lastCoord.LocalPlanet, MaxVelocity).ToFloat3();

                var lastRotation = Quaternion.LookRotation(_lastCoord.LocalPlanet);
                var targetRotation = Quaternion.LookRotation(currentCoord.LocalPlanet);
                plate.Velocity = Quaternion.Inverse(plate.Rotation) * Quaternion.LookRotation(_lastCoord.LocalPlanet) * Quaternion.Inverse(targetRotation);

                _lastCoord.LocalPlanet += (currentCoord.LocalPlanet - _lastCoord.LocalPlanet);
                _lastCoord.Altitude = Coordinate.PlanetRadius;

                //RotationScaling?
                /*
                                 var coord = new Coordinate(hit.point, Planet.LocalToWorld);
                var lastPoint = _lastCoord.LocalPlanet;
                var motionVector = Vector3.ClampMagnitude(lastPoint - coord.LocalPlanet, MaxVelocity);

                var forward = motionVector.normalized;
                var up = Planet.LocalToWorld.Rotation.ToQuaternion() * Camera.main.transform.position;
                var right = Quaternion.AngleAxis(-90, forward) * up;
                var distanceToAngle = (2 * math.PI * Coordinate.PlanetRadius) / 360f;
                var angle = motionVector.magnitude / distanceToAngle;

                plate.Velocity = Quaternion.AngleAxis(angle, right);
                _lastCoord.LocalPlanet += motionVector.ToFloat3(); 
                _lastCoord.Altitude = Coordinate.PlanetRadius;
                 
                 */

            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            _currentPlateId = 0;
        }
    }
}
