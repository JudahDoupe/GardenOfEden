﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private List<CapturePoint> _capturePoints;
    private bool _isGameInProgress;

    private void Start()
    {
        _isGameInProgress = true;
        _capturePoints = FindObjectsOfType<CapturePoint>().ToList();
    }

    private void Update()
    {
        if(_capturePoints.All(x => x.IsCaptured) && _isGameInProgress)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        _isGameInProgress = false;

        var player = FindObjectOfType<FirstPersonController>();
        player.IsCameraMovable = false;
        player.IsPlayerMovable = false;
        player.IsFocusEnabled = false;
        player.IsMouseHidden = false;
        var camera = FindObjectOfType<CameraController>();
        var mapCeneter = GameObject.Find("MapCenter");
        camera.IsInUse = true;
        camera.RotateAround(mapCeneter.transform.position, new Vector3(0, 50, -100));
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
