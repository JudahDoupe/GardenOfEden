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
        // AddButtonAction("Settings", () => { }); TODO
        AddButtonAction("DeletePlanet", () =>
        {
            HideUi();
            ShowDeletePlanetDialog();
        });
        AddButtonAction("Continue", () =>
        {
            DisableMainMenu();
            ToolbarController.EnableToolbar();
        });
        AddButtonAction("NewPlanet", () =>
        {
            if (string.IsNullOrWhiteSpace(Instance._playerData.CurrentPlanetName))
            {
                Instance.HideUi();
                Instance.ShowNewPlanetDialog();
            }
            else
            {
                Instance.UnloadPlanet(onUnload: Instance.ShowNewPlanetDialog);
            }
        });
        AddButtonAction("NextPlanet", () =>
        {
            var planets = Instance._playerData.PlanetNames;
            var nextPlanet = planets[planets.IndexOf(Instance._playerData.CurrentPlanetName) + 1];
            Instance.UnloadPlanet(true, () => Instance.LoadPlanet(nextPlanet));
        });
        AddButtonAction("PrevPlanet", () =>
        {
            var planets = Instance._playerData.PlanetNames;
            var prevPlanet = planets[planets.IndexOf(Instance._playerData.CurrentPlanetName) - 1];
            Instance.UnloadPlanet(false, () => Instance.LoadPlanet(prevPlanet, false));
        });
        AddButtonAction("ConfirmNewPlanet", () =>
        {
            var planetName = Instance.UI.rootVisualElement.Query<TextField>("NewPlanetInput").First().value;
            if (string.IsNullOrWhiteSpace(planetName) || Instance._playerData.PlanetNames.Contains(planetName))
            {
                Instance.ShowNewPlanetNameError();
            }
            else
            {
                Instance.HideNewPlanetNameError();
                Instance.HideNewPlanetDialog();
                Instance.LoadPlanet(planetName);
            }
        });
        AddButtonAction("CancelNewPlanet", () =>
        {
            Instance.HideNewPlanetNameError();
            Instance.HideNewPlanetDialog();
            if (string.IsNullOrWhiteSpace(Instance._playerData.CurrentPlanetName))
                Instance.ShowUi();
            else
                Instance.LoadPlanet(Instance._playerData.CurrentPlanetName, false);
        });
        AddButtonAction("ConfirmDeletePlanet", () =>
        {
            HideDeletePlanetDialog();
            Instance.DeletePlanet(Instance._playerData.CurrentPlanetName);
        });
        AddButtonAction("CancelDeletePlanet", () =>
        {
            HideDeletePlanetDialog();
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
        CameraController.SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Smooth);
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
        ShowPlanetNavigation();
        ShowPlanetName();
        ShowPlanetControls();
    }
    private void HideUi()
    {
        HidePlanetNavigation();
        HidePlanetName();
        HidePlanetControls();
    }

    private void ShowPlanetNavigation()
    {
        if (!_playerData.PlanetNames.Any())
            return;

        if (_playerData.CurrentPlanetName != _playerData.PlanetNames.First())
            UI.rootVisualElement.Query("Left").First().RemoveFromClassList("Hidden");

        if (_playerData.CurrentPlanetName != _playerData.PlanetNames.Last())
            UI.rootVisualElement.Query("Right").First().RemoveFromClassList("Hidden");
    }
    private void HidePlanetNavigation()
    {
        UI.rootVisualElement.Query("Left").First().AddToClassList("Hidden");
        UI.rootVisualElement.Query("Right").First().AddToClassList("Hidden");
    }

    private void ShowPlanetName()
    {
        if (string.IsNullOrWhiteSpace(_playerData.CurrentPlanetName))
            return;

        UI.rootVisualElement.Query("Header").First().RemoveFromClassList("Hidden");
    }
    private void HidePlanetName() => UI.rootVisualElement.Query("Header").First().AddToClassList("Hidden");

    private void ShowPlanetControls()
    {
        UI.rootVisualElement.Query("Footer").First().RemoveFromClassList("Hidden");

        if (string.IsNullOrWhiteSpace(_playerData.CurrentPlanetName))
        {
            UI.rootVisualElement.Query("DeletePlanet").First().AddToClassList("Hidden");
            UI.rootVisualElement.Query("Continue").First().AddToClassList("Hidden");
        }
        else
        {
            UI.rootVisualElement.Query("DeletePlanet").First().RemoveFromClassList("Hidden");
            UI.rootVisualElement.Query("Continue").First().RemoveFromClassList("Hidden");
        }
    }
    private void HidePlanetControls() => UI.rootVisualElement.Query("Footer").First().AddToClassList("Hidden");

    private void ShowNewPlanetDialog() => UI.rootVisualElement.Query("NewPlanetContainer").First().RemoveFromClassList("Hidden");
    private void HideNewPlanetDialog() => UI.rootVisualElement.Query("NewPlanetContainer").First().AddToClassList("Hidden");
    private void ShowNewPlanetNameError() => UI.rootVisualElement.Query("NewPlanetInput").First().AddToClassList("Error");
    private void HideNewPlanetNameError() => UI.rootVisualElement.Query("NewPlanetInput").First().RemoveFromClassList("Error");
    
    private void ShowDeletePlanetDialog() => UI.rootVisualElement.Query("DeletePlanetDialog").First().RemoveFromClassList("Hidden");
    private void HideDeletePlanetDialog() => UI.rootVisualElement.Query("DeletePlanetDialog").First().AddToClassList("Hidden");

    #endregion
}