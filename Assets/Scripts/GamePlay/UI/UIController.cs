using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private FirstPersonController _player;
    private GameObject _pauseMenu;
    private GameObject _statsMenu;

    private void Start()
    {
        _player = GetComponentInParent<FirstPersonController>();
        _pauseMenu = transform.Find("Pause Menu").gameObject;
        _statsMenu = transform.Find("Stats").gameObject;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_pauseMenu.activeSelf)
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
        _pauseMenu.SetActive(true);
        _player.IsCameraMovable = false;
        _player.IsPlayerMovable = false;
        _player.IsFocusEnabled = false;
        _player.IsMouseHidden = false;
    }

    public void HidePauseMenu()
    {
        _pauseMenu.SetActive(false);
        _player.IsCameraMovable = true;
        _player.IsPlayerMovable = true;
        _player.IsFocusEnabled = true;
        _player.IsMouseHidden = true;
    }

    public void ShowStatsMenu()
    {
        _statsMenu.transform.Find("Plants").transform.Find("Text").GetComponent<Text>().text = PlantApi.GetTotalPlantPopulation().ToString();
        _statsMenu.SetActive(true);
        _player.IsCameraMovable = false;
        _player.IsPlayerMovable = false;
        _player.IsFocusEnabled = false;
        _player.IsMouseHidden = false;
    }

    public void HideStatsMenu()
    {
        _statsMenu.SetActive(false);
        _player.IsCameraMovable = true;
        _player.IsPlayerMovable = true;
        _player.IsFocusEnabled = true;
        _player.IsMouseHidden = true;
    }
}
