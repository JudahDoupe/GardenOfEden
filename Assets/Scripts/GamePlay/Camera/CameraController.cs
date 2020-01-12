using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraController : MonoBehaviour
{
    public float MoveSpeed = 0.3f;
    public float LookSpeed = 1;

    /* INNER MECHINATIONS */

    private Transform _focusedObject { get; set; }
    private Transform _focusedGoal { get; set; }

    private bool _isDrifting = true;
    private Vector3 _target;
    private Vector3 _targetLook;
    private float _horizontalRatio;

    private GrowthService _growthService;
    private LandService _landService;
    private GameService _gameService;

    private CameraMode currentMode = CameraMode.Cinematic;
    private enum CameraMode
    {
        Cinematic,
        BirdsEye,
    }

    private void Start()
    {
        _growthService = FindObjectOfType<GrowthService>();
        _landService = FindObjectOfType<LandService>();
        _gameService = FindObjectOfType<GameService>();

        _growthService.NewPlantSubject.Subscribe(NewPlantAction);
        _gameService.PointCapturedSubject.Subscribe(PointCapturedAction);

        _focusedObject = _gameService.FocusedPlant.transform;
        _focusedGoal = GetNearestGoal()?.transform;
    }
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentMode = currentMode == CameraMode.Cinematic ? CameraMode.BirdsEye : CameraMode.Cinematic;
        }

        switch (currentMode)
        {
            case CameraMode.Cinematic:
                UpdateCinematic();
                break;
            case CameraMode.BirdsEye:
                UpdateBirdsEye();
                break;
        }
    }

    private void UpdateCinematic()
    {
        MoveSpeed = 0.25f;
        LookSpeed = 0.75f;
        if (_focusedObject != null)
        {
            UpdateTargets();
        }

        LerpTowardTargets();
    }

    private void UpdateBirdsEye()
    {
        MoveSpeed = 1f;
        LookSpeed = 10;
        _target = new Vector3(0, 350, -100);
        _targetLook = new Vector3(0, 0, -99);

        LerpTowardTargets();
    }

    private void UpdateTargets()
    {
        var bounds = _focusedObject.GetBounds();
        var plantPosition = bounds.center;
        var direction = _focusedGoal != null ?
            (plantPosition - _focusedGoal.position).normalized :
            (transform.position - plantPosition).normalized;
        var distance = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) * (145f / Camera.main.fieldOfView);
        var horizontalOffset = new Vector3(distance * _horizontalRatio, 0, 0);

        _target = plantPosition + (direction * distance) + horizontalOffset;
        _targetLook = plantPosition + horizontalOffset / 2;
    }
    private void UpdateHorizontalRatio()
    {
        var horizontalRatios = new[] { -0.66f, -0.5f, 0, 0.5f, 0.66f };
        _horizontalRatio = horizontalRatios[Mathf.RoundToInt(Random.Range(0, 4))];
    }

    private void LerpTowardTargets()
    {
        var targetPosition = Vector3.Lerp(transform.position, _target, MoveSpeed * Time.deltaTime);
        targetPosition.y = Mathf.Max(targetPosition.y, _landService.SampleTerrainHeight(targetPosition) + 0.5f);
        transform.position = targetPosition;

        var targetRotation = Quaternion.LookRotation(_targetLook - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, LookSpeed * Time.deltaTime);
    }

    private CapturePoint GetNearestGoal()
    {
        var capturePoints = FindObjectsOfType<CapturePoint>().Where(x => !x.IsCaptured);
        return capturePoints.Any()
            ? capturePoints.Aggregate((curMin, x) =>
                curMin == null ||
                Vector3.Distance(x.transform.position, transform.position) < Vector3.Distance(curMin.transform.position, transform.position) ? x : curMin)
            : null;
    }

    private void NewPlantAction(Plant plant)
    {
        if (_isDrifting && 
            ((_focusedGoal != null &&
            _focusedObject != null &&
            Vector3.Distance(_focusedGoal.position, plant.transform.position) < Vector3.Distance(_focusedGoal.position, _focusedObject.position)) 
            || _focusedObject == null))
        {
            _focusedObject = plant.transform;
            UpdateHorizontalRatio();
        }
    }

    private void PointCapturedAction(CapturePoint cp)
    {
        StartCoroutine(FocusOnCapturePoint(cp));
    }
    private IEnumerator FocusOnCapturePoint(CapturePoint cp)
    {
        _focusedGoal = GetNearestGoal().transform;
        _focusedObject = cp.transform;
        UpdateHorizontalRatio();
        _isDrifting = false;

        yield return new WaitForSeconds(5);

        _isDrifting = true;
    }
}
