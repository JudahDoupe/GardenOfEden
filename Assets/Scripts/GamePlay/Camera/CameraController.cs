using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MoveSpeed = 0.3f;
    public float LookSpeed = 1;
    public float Distance = 5;

    public void FocusOnPlant(Plant plant)
    {
        FocusedPlant = plant;
        FocusedGoal = FindObjectsOfType<CapturePoint>().Aggregate((curMin, x) =>
                curMin == null || 
                Vector3.Distance(x.transform.position, plant.transform.position) < Vector3.Distance(curMin.transform.position, plant.transform.position) ? x : curMin);

        UpdateHorizontalRatio();
        UpdateCinematicTargets();
    }


    /* INNER MECHINATIONS */

    public Plant FocusedPlant { get; set; }
    public CapturePoint FocusedGoal { get; set; }

    private Vector3 _target;
    private Vector3 _targetLook;
    private float _horizontalRatio;

    private GrowthService _growthService;
    private SoilService _soilService;

    private void Start()
    {
        _growthService = FindObjectOfType<GrowthService>();
        _soilService = FindObjectOfType<SoilService>();
        FocusOnPlant(FindObjectsOfType<Plant>().First());
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
            UpdateCinematicTargets();
        }

        var targetPosition = Vector3.Lerp(transform.position, _target, MoveSpeed * Time.deltaTime);
        targetPosition.y = Mathf.Max(targetPosition.y, _soilService.SampleTerrainHeight(targetPosition) + 0.5f);
        transform.position = targetPosition;

        var targetRotation = Quaternion.LookRotation(_targetLook - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, LookSpeed * Time.deltaTime);
    }

    private void UpdateCinematicTargets()
    {
        var plantPosition = FocusedPlant.transform.GetBounds().center;
        var direction = (plantPosition - FocusedGoal.transform.position).normalized;
        var horizontalOffset = new Vector3(Distance * _horizontalRatio, 0, 0);

        _target = plantPosition + (direction * Distance) + horizontalOffset;
        _targetLook = plantPosition + horizontalOffset / 2;
    }
    private float UpdateHorizontalRatio()
    {
        var horizontalRatios = new[] { -0.66f, -0.5f, 0, 0.5f, 0.66f };
        return horizontalRatios[Mathf.RoundToInt(Random.Range(0, 4))];
    }
}
