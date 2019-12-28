using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MoveSpeed;
    public float LookSpeed;
    public float Distance = 5;
    public Plant FocusedPlant;

    public void MoveTo(Vector3 position)
    {
        _target = position;
        _rotateAroundPoint = null;
        _rotateAroundOffset = null;
    }

    public void LookAt(Vector3 position)
    {
        _targetLook = position;
    }

    public void RotateAround(Vector3 position, Vector3 offset)
    {
        _rotateAroundPoint = position;
        _rotateAroundOffset = offset;
        _targetLook = position;
    }

    public void FocusPlant(Plant plant)
    {
        FocusedPlant = plant;
        RotateAround(plant.transform.position + new Vector3(0, .5f, 0), new Vector3(0, 2, -5));
    }

    /* INNER MECHINATIONS */

    private Vector3 _target;
    private Vector3 _targetLook;

    private Vector3? _rotateAroundPoint;
    private Vector3? _rotateAroundOffset;

    private GrowthService _growthService;
    private SoilService _soilService;

    private Vector3 direction;
    private void Start()
    {
        _growthService = FindObjectOfType<GrowthService>();
        _soilService = FindObjectOfType<SoilService>();
        FocusPlant(FindObjectsOfType<Plant>().First());
    }
    private void Update()
    {
        Distance = Mathf.Clamp(Distance - Input.GetAxis("Mouse ScrollWheel") * MoveSpeed, 1f, 30f);
    }
    private void LateUpdate()
    {
        if (FocusedPlant.IsAlive)
        {
            _growthService.PrioritizePlant(FocusedPlant);
        }

        if (_rotateAroundPoint.HasValue)
        {
            RotateTargetAroundPoint();
        }

        var targetPosition = Vector3.Lerp(transform.position, _target, MoveSpeed * Time.deltaTime);
        targetPosition.y = Mathf.Max(targetPosition.y, _soilService.SampleTerrainHeight(targetPosition) + 0.5f);
        transform.position = targetPosition;

        var targetRotation = Quaternion.LookRotation(_targetLook - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, LookSpeed * Time.deltaTime);
    }

    private float currentAngle = 0;
    private void RotateTargetAroundPoint()
    {
        var circumfrance = _rotateAroundOffset.Value.z * _rotateAroundOffset.Value.z * Mathf.PI;
        var angleChange = (MoveSpeed / circumfrance) * 360;
        var clampedAngleChange = Mathf.Min(angleChange, Time.deltaTime * 10);
        currentAngle = (currentAngle  + clampedAngleChange) % 360;
        var offset = Quaternion.AngleAxis(currentAngle, Vector3.up) * _rotateAroundOffset.Value;

        _target = _rotateAroundPoint.Value + offset;
        _rotateAroundOffset = _rotateAroundOffset.Value.normalized * Distance;
    }
}
