using System;
using System.Linq;
using Assets.GamePlay.Cameras;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Application = UnityEngine.Device.Application;

public class MainMenuController : Singleton<MainMenuController>
{
    public UIDocument UI;

    private PlayerData _playerData;

    private void Start()
    {
        this.RunTaskInCoroutine(PlayerDataStore.GetOrCreate(), () =>
        {
            _playerData = PlayerDataStore.GetOrCreate().Result;
            if (string.IsNullOrWhiteSpace(_playerData.CurrentPlanetName))
                ShowUi();
            else
                LoadPlanet(_playerData.CurrentPlanetName);
        });

        AddButtonAction("Quit", Application.Quit);
        AddButtonAction("Settings", () =>
        {
            DisableMainMenu();
            SettingsController.ShowSettingsMenu();
        });
        AddButtonAction("DeletePlanet", () =>
        {
            HideUi();
            ShowUi("DeletePlanetDialog");
        });
        AddButtonAction("Continue", () =>
        {
            DisableMainMenu();
            ToolbarController.EnableToolbar();
        });
        AddButtonAction("NewPlanet", () =>
        {
            if (string.IsNullOrWhiteSpace(_playerData.CurrentPlanetName))
            {
                HideUi();
                ShowUi("NewPlanetContainer");
            }
            else
            {
                UnloadPlanet(onUnload: () => ShowUi("NewPlanetContainer"));
            }
        });
        AddButtonAction("NextPlanet", () =>
        {
            var planets = _playerData.PlanetNames;
            var nextPlanet = planets[planets.IndexOf(_playerData.CurrentPlanetName) + 1];
            UnloadPlanet(true, () => Instance.LoadPlanet(nextPlanet));
        });
        AddButtonAction("PrevPlanet", () =>
        {
            var planets = _playerData.PlanetNames;
            var prevPlanet = planets[planets.IndexOf(_playerData.CurrentPlanetName) - 1];
            UnloadPlanet(false, () => LoadPlanet(prevPlanet, false));
        });
        AddButtonAction("ConfirmNewPlanet", () =>
        {
            var planetName = UI.rootVisualElement.Query<TextField>("NewPlanetInput").First().value;
            if (string.IsNullOrWhiteSpace(planetName) || _playerData.PlanetNames.Contains(planetName))
            {
                ShowUiError("NewPlanetInput");
            }
            else
            {
                HideUiError("NewPlanetInput");
                HideUi("NewPlanetContainer");
                LoadPlanet(planetName);
            }
        });
        AddButtonAction("CancelNewPlanet", () =>
        {
            HideUiError("NewPlanetInput");
            HideUi("NewPlanetContainer");
            if (string.IsNullOrWhiteSpace(Instance._playerData.CurrentPlanetName))
                ShowUi();
            else
                LoadPlanet(Instance._playerData.CurrentPlanetName, false);
        });
        AddButtonAction("ConfirmDeletePlanet", () =>
        {
            HideUi("DeletePlanetDialog");
            DeletePlanet(Instance._playerData.CurrentPlanetName);
        });
        AddButtonAction("CancelDeletePlanet", () =>
        {
            HideUi("DeletePlanetDialog");
            ShowUi();
        });

        void AddButtonAction(string buttonName, Action action)
        {
            UI.rootVisualElement.Query(buttonName).First().Query<Button>(classes: "Button").First().clicked += action;
        }
    }

    public static void EnableMainMenu()
    {
        Instance.ShowUi();
        CameraController.TransitionToMainMenuCamera(CameraTransition.Smooth);
    }

    public static void DisableMainMenu()
    {
        Instance.HideUi();
    }

    #region Planet Helpers

    private void UnloadPlanet(bool exitLeft = true, Action onUnload = null)
    {
        HideUi();
        StartCoroutine(AnimationUtils.AnimateVector3(
            1,
            Planet.Transform.position,
            Camera.main.transform.right * (exitLeft ? -7000 : 7000),
            x => Planet.Transform.position = x,
            onUnload,
            EaseType.In));
    }

    private void LoadPlanet(string planetName, bool enterRight = true)
    {
        HideUi();
        Planet.Transform.position = Camera.main.transform.right * (enterRight ? 7000 : -7000);
        Planet.Instance.Load(planetName, () =>
        {
            _playerData.CurrentPlanetName = planetName;
            if (!_playerData.PlanetNames.Contains(planetName)) _playerData.PlanetNames.Add(planetName);
            PlayerDataStore.Update(_playerData).ConfigureAwait(false);

            UI.rootVisualElement.Query<Label>("PlanetName").First().text = planetName;
            StartCoroutine(AnimationUtils.AnimateVector3(
                1,
                Planet.Transform.position,
                Vector3.zero,
                x => Planet.Transform.position = x,
                ShowUi,
                EaseType.Out));
        });
    }

    private void DeletePlanet(string planetName)
    {
        var index = _playerData.PlanetNames.IndexOf(Instance._playerData.CurrentPlanetName);
        var isFirst = index == 0;
        var isLast = index == _playerData.PlanetNames.Count - 1;
        var nextPlanet = isFirst
            ? isLast
                ? null
                : _playerData.PlanetNames[1]
            : _playerData.PlanetNames[index - 1];
        
        HideUi();
        _playerData.PlanetNames.Remove(planetName);
        PlanetDataStore.Delete(planetName);
        PlayerDataStore.Update(_playerData).ConfigureAwait(false);

        StartCoroutine(AnimationUtils.AnimateVector3(
            1,
            Planet.Transform.position,
            new Vector3(0, -5000, 0),
            x => Planet.Transform.position = x,
            TryLoadNextPlanet,
            EaseType.InExp));

        void TryLoadNextPlanet()
        {
            if (nextPlanet == null)
            {
                _playerData.CurrentPlanetName = null;
                PlayerDataStore.Update(_playerData).ConfigureAwait(false);
                ShowUi();
            }
            else
                LoadPlanet(nextPlanet, enterRight: isFirst);
        }
    }

    #endregion

    #region UI Helpers

    private void ShowUi()
    {
        ShowUi("Header");
        ShowUi("Footer");

        if (string.IsNullOrWhiteSpace(_playerData.CurrentPlanetName))
        {
            HideUi("PlanetName");
            HideUi("DeletePlanet");
            HideUi("Continue");
        }
        else
        {
            ShowUi("PlanetName");
            ShowUi("DeletePlanet");
            ShowUi("Continue");
        }

        if (_playerData.PlanetNames.Any() && _playerData.CurrentPlanetName != _playerData.PlanetNames.First())
            ShowUi("Left");

        if (_playerData.PlanetNames.Any() && _playerData.CurrentPlanetName != _playerData.PlanetNames.Last())
            ShowUi("Right");
        
    }
    private void HideUi()
    {
        HideUi("Left");
        HideUi("Right");
        HideUi("Footer");
        HideUi("Header");
    }

    private void ShowUi(string uiaNme) => UI.rootVisualElement.Query(uiaNme).First().RemoveFromClassList("Hidden");
    private void HideUi(string uiaNme) => UI.rootVisualElement.Query(uiaNme).First().AddToClassList("Hidden");
    
    private void ShowUiError(string uiaNme) => UI.rootVisualElement.Query(uiaNme).First().RemoveFromClassList("Error");
    private void HideUiError(string uiaNme) => UI.rootVisualElement.Query(uiaNme).First().AddToClassList("Error");
    
    #endregion
}