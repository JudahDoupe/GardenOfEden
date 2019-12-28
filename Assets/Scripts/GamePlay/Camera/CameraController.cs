using System.Collections;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MoveSpeed;
    public float LookSpeed;
    public Plant FocusedPlant;

    public void MoveTo(Vector3 position)
    {
        _targetPosition = position;
        _rotateAroundPoint = null;
        _rotateAroundOffset = null;
    }

    public void LookAt(Vector3 position)
    {
        _targetLookPosition = position;
    }

    public void RotateAround(Vector3 position, Vector3 offset)
    {
        _rotateAroundPoint = position;
        _rotateAroundOffset = offset;
        _targetLookPosition = position;
    }

    public void FocusPlant(Plant plant)
    {
        FocusedPlant = plant;
        RotateAround(plant.transform.position + new Vector3(0, .5f, 0), new Vector3(0, 2, -5));
    }

    /* INNER MECHINATIONS */

    private Vector3 _targetPosition;
    private Vector3 _targetLookPosition;

    private Vector3? _rotateAroundPoint;
    private Vector3? _rotateAroundOffset;

    private GrowthService _growthService;

    private float currentAngle = 0;
    private void Start()
    {
        _growthService = FindObjectOfType<GrowthService>();
        FocusPlant(FindObjectsOfType<Plant>().First());
    }
    private void LateUpdate()
    {
        if (FocusedPlant.IsAlive)
        {
            _growthService.PrioritizePlant(FocusedPlant);
        }

        if (_rotateAroundPoint.HasValue)
        {
            currentAngle = (currentAngle + (Time.deltaTime * MoveSpeed * _rotateAroundOffset.Value.z * 0.1f * Mathf.PI)) % 360;
            var offset = Quaternion.AngleAxis(currentAngle, Vector3.up) * _rotateAroundOffset;
            _targetPosition = _rotateAroundPoint.Value + offset.Value;
        }

        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime);

        var targetRotation = Quaternion.LookRotation(_targetLookPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, LookSpeed * Time.deltaTime);
    }
}
