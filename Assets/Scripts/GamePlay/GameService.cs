using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using UnityEngine;

public class GameService : MonoBehaviour
{
    public bool IsGameInProgress { get; private set; }
    public Plant FocusedPlant { get; set; }
    public Subject<CapturePoint> PointCapturedSubject = new Subject<CapturePoint>();

    private void Start()
    {
        IsGameInProgress = true;
        PointCapturedSubject.Subscribe(PointCapturedAction);
        FocusedPlant = FindObjectsOfType<Plant>().First();
    }

    private void PointCapturedAction(CapturePoint cp)
    {
        if (FindObjectsOfType<CapturePoint>().All(x => x.IsCaptured))
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        IsGameInProgress = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public CapturePoint GetNearestGoal()
    {
        var capturePoints = FindObjectsOfType<CapturePoint>().Where(x => !x.IsCaptured);
        return capturePoints.Any()
            ? capturePoints.Closest(Camera.main.transform.position)
            : null;
    }
}
