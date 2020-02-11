using System;
using System.Collections;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    public class Focus
    {
        private Transform _object;
        private Vector2 _offsetRatio;

        public Transform Object
        {
            get => _object;
            set
            {
                var offsetRatios = new[] { -0.66f, -0.5f, 0, 0.5f, 0.66f };
                var ratio = offsetRatios[Mathf.RoundToInt(UnityEngine.Random.Range(0, 4))];
                _offsetRatio = new Vector2(ratio, 0);
                _object = value;
            }
        }
        public Vector3 Position(float cameraDistance)
        {
            if (_object == null)
            {
                return Camera.main.transform.position;
            }
            else
            {
                var offset = Camera.main.transform.TransformVector(new Vector3(cameraDistance * _offsetRatio.x, 0, 0));
                return offset + _object.GetBounds().center;
            }
        }
        public bool IsDrifting = true;

    }

    public Focus PrimaryFocus { get; private set; } = new Focus();
    public Focus SecondaryFocus { get; private set; } = new Focus();

    void Start()
    {
        PrimaryFocus.Object = DI.GameService.FocusedPlant.transform;
        SecondaryFocus.Object = DI.GameService.GetNearestGoal()?.transform;

        DI.GrowthService.NewPlantSubject.Subscribe(NewPlantAction);
        DI.GameService.PointCapturedSubject.Subscribe(PointCapturedAction);
    }

    public void HoldFocus(Focus focus, Transform target, TimeSpan time)
    {
        StartCoroutine(HoldFocusAsync(focus, target, time));
    }

    private IEnumerator HoldFocusAsync(Focus focus, Transform target, TimeSpan time)
    {
        focus.IsDrifting = false;
        focus.Object = target;

        yield return new WaitForSeconds((float) time.TotalSeconds);

        focus.IsDrifting = true;
    }


    private void PointCapturedAction(CapturePoint cp)
    {
        if (SecondaryFocus.IsDrifting)
        {
            HoldFocus(SecondaryFocus, cp.transform, TimeSpan.FromSeconds(5));
        }
    }
    private void NewPlantAction(Plant plant)
    {
        var plantPos = plant.transform.position;
        var primaryPos = PrimaryFocus.Object?.position ?? plantPos;
        var secondaryPos = SecondaryFocus.Object?.position ?? plantPos;
        if (PrimaryFocus.IsDrifting && Vector3.Distance(plantPos, secondaryPos) <= Vector3.Distance(primaryPos, secondaryPos))
        {
            PrimaryFocus.Object = plant.transform;
        }
    }
}
