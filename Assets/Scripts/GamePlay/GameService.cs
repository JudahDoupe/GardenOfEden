using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using UnityEngine;

public class GameService : MonoBehaviour
{
    public bool IsGameInProgress { get; private set; }
    public Plant FocusedPlant { get; set; }
    public Subject<CapturePoint> PointCapturedSubject = new Subject<CapturePoint>();

    private List<CapturePoint> _capturePoints;

    private void Start()
    {
        _capturePoints = FindObjectsOfType<CapturePoint>().ToList();
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

        var ui = FindObjectOfType<UIController>();
        ui.ShowStatsMenu();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
