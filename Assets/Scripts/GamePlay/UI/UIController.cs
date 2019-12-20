using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private Canvas _canvas;
    private FirstPersonController _player;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _player = GetComponentInParent<FirstPersonController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_canvas.enabled)
            {
                HidePauseMenu();
            }
            else
            {
                ShowPauseMenu();
            }
        }
    }

    public void ShowPauseMenu()
    {
        _canvas.enabled = true;
        _player.IsCameraMovable = false;
        _player.IsPlayerMovable = false;
        _player.IsFocusEnabled = false;
        _player.IsMouseHidden = false;
    }

    public void HidePauseMenu()
    {
        _canvas.enabled = false;
        _player.IsCameraMovable = true;
        _player.IsPlayerMovable = true;
        _player.IsFocusEnabled = true;
        _player.IsMouseHidden = true;
    }
}
