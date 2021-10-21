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
    private GameObject _ball;

    public void Enable()
    {
        _ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _ball.transform.localScale = new Vector3(25, 25, 25);
        _ball.GetComponent<SphereCollider>().enabled = false;
        _isActive = true;
    }
    public void Disable()
    {
        Destroy(_ball);
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
            if (Physics.Raycast(ray, out var hit, 10000, LayerMask.GetMask("Planet")))
            {
                _lastCoord = new Coordinate(hit.transform.InverseTransformPoint(hit.point));
                _lastCoord.Altitude = Coordinate.PlanetRadius;
                _currentPlateId = (int) math.round(EnvironmentDataStore.ContinentalIdMap.Sample(_lastCoord).r);
            }
            else {
                _currentPlateId = 0;
            }
        }
        if (Input.GetMouseButton(0) && _currentPlateId > 0)
        {
            // Raycasting seems to jitter for some reason
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 10000, LayerMask.GetMask("Planet")))
            {
                _ball.transform.position = hit.point;
                var plate = Singleton.PlateTectonics.Plates.Single(x => x.Id == _currentPlateId);

                var hitCoord = new Coordinate(hit.transform.InverseTransformPoint(hit.point));
                var motionVector = Vector3.ClampMagnitude(hitCoord.LocalPlanet - _lastCoord.LocalPlanet, MaxVelocity).ToFloat3();
                var currentCoord = new Coordinate(_lastCoord.LocalPlanet + motionVector);

                var lastRotation = Quaternion.LookRotation(_lastCoord.LocalPlanet);
                var targetRotation = Quaternion.LookRotation(currentCoord.LocalPlanet);
                plate.Velocity = targetRotation * Quaternion.Inverse(lastRotation);

                _lastCoord.LocalPlanet = currentCoord.LocalPlanet;
                _lastCoord.Altitude = Coordinate.PlanetRadius;

            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            _currentPlateId = 0;
        }
    }
}
