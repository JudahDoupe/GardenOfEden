using System;
using System.Collections;
using System.Linq;
using Assets.GamePlay.Cameras;
using Unity.Mathematics;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    public Camera Camera;
    public Transform Focus;
    private static CameraPerspective _perspective;
    
    
    public static void TransitionToMainMenuCamera(CameraTransition transition) => SetPerspective(FindObjectOfType<MainMenuCamera>(), transition);
    public static void TransitionToSatelliteCamera(CameraTransition transition) => SetPerspective(FindObjectOfType<SatelliteCamera>(), transition);
    public static void TransitionToLandscapeCamera(CameraTransition transition) => SetPerspective(FindObjectOfType<LandscapeCamera>(), transition);

    

    private void Start()
    {
        SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Instant);
    }
    
    private static void SetPerspective(CameraPerspective perspective, CameraTransition transition)
    {
        if (perspective == _perspective) return;

        if (_perspective != null) _perspective.Disable();
        _perspective = perspective;

        TransitionState(perspective.StartTransitionTo(), transition, () => { _perspective.Enable(); });
    }

    private static void TransitionState(CameraState end, CameraTransition transition, Action callback = null)
    {
        Instance.StopAllCoroutines();
        if (transition.Speed <= 0)
        {
            CameraUtils.SetState(end);
            callback();
        }
        else
        {
            end.Camera.transform.parent = end.CameraParent;
            end.Focus.parent = end.FocusParent;
            Cursor.lockState = end.Cursor;
            var start = new CameraState(Instance.Camera, Instance.Focus);

            var speeds = new[]
            {
                GetTransitionTime(start.CameraLocalPosition, end.CameraLocalPosition, transition.Speed),
                GetTransitionTime(start.CameraLocalRotation, end.CameraLocalRotation, transition.Speed),
                GetTransitionTime(start.FocusLocalPosition, end.FocusLocalPosition, transition.Speed),
                GetTransitionTime(start.FocusLocalRotation, end.FocusLocalRotation, transition.Speed),
                GetTransitionTime(start.FieldOfView, end.FieldOfView, transition.Speed)
            };
            Instance.StartCoroutine(AnimateTransition(speeds.Max(), start, end, callback, transition.Ease));
        }
    }

    private static IEnumerator AnimateTransition(float seconds, CameraState start, CameraState end, Action callback, EaseType ease)
    {
        var remainingSeconds = seconds;
        var t = 0f;

        while (t < 1)
        {
            yield return new WaitForEndOfFrame();

            var lerp = ease.LerpValue(t);
            end.Camera.transform.localPosition = Vector3.Lerp(start.CameraLocalPosition, end.CameraLocalPosition, lerp);
            end.Camera.transform.position = new Coordinate(end.Camera.transform.position, Planet.LocalToWorld).ClampAboveTerrain().Global;
            end.Camera.transform.localRotation = Quaternion.Lerp(start.CameraLocalRotation, end.CameraLocalRotation, lerp);
            end.Focus.localPosition = Vector3.Lerp(start.FocusLocalPosition, end.FocusLocalPosition, lerp);
            end.Focus.localRotation = Quaternion.Lerp(start.FocusLocalRotation, end.FocusLocalRotation, lerp);
            end.Camera.fieldOfView = math.lerp(start.FieldOfView, end.FieldOfView, lerp);

            remainingSeconds -= Time.deltaTime;
            t = 1 - remainingSeconds / seconds;
        }

        CameraUtils.SetState(end);

        callback?.Invoke();
    }

    private static float GetTransitionTime(Vector3 start, Vector3 end, float transitionSpeed = 1) => DistanceToTransitionTime(Vector3.Distance(start, end), transitionSpeed);
    private static float GetTransitionTime(Quaternion start, Quaternion end, float transitionSpeed = 1) => DistanceToTransitionTime(Quaternion.Angle(start, end), transitionSpeed);
    private static float GetTransitionTime(float start, float end, float transitionSpeed = 1) => DistanceToTransitionTime(math.abs(start - end), transitionSpeed);
    private static float DistanceToTransitionTime(float distance, float transitionSpeed) => math.sqrt(distance) * 0.05f / transitionSpeed;
}