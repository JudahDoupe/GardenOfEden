using System;
using Assets.GamePlay.Cameras;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : Singleton<MainMenuController>
{
    public UIDocument UI;

    private void Start()
    {
        AddButtonAction("Continue", Continue);
        
        LoadPlanet("Earth");
        
        void AddButtonAction(string buttonName, Action action)
            => UI.rootVisualElement.Query(buttonName).First().Query<Button>(classes: "Button").First().clicked += action;
    }
    
    // Menu
    
    public static void EnableMainMenu()
    {
        Instance.ShowUi();
        CameraController.SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Smooth);
    }
    public static void DisableMainMenu() => Instance.HideUi();

    // Buttons
    
    public static void Continue() {
        DisableMainMenu();
        ToolbarController.EnableToolbar();
    }

    
    // Planet Helpers

    private void UnloadPlanet(bool exitRight = true, Action onUnload = null)
    {
        HideUi();
        StartCoroutine(AnimationUtils.AnimateVector3(1,
                                                     Planet.Transform.position,
                                                     new Vector3(exitRight ? 5000 : -5000, 0, 0), 
                                                     x => Planet.Transform.position = x,
                                                     onUnload,
                                                     EaseType.In));
    }
    private void LoadPlanet(string planetName, bool enterRight = true)
    {
        HideUi();
        Planet.Transform.position = new Vector3(enterRight ? 5000 : -5000, 0, 0);
        Planet.Instance.Load(planetName, () =>
        {
            UI.rootVisualElement.Query<Label>("PlanetName").First().text = planetName;
            StartCoroutine(AnimationUtils.AnimateVector3(1,
                                                         Planet.Transform.position,
                                                         Vector3.zero, 
                                                         x => Planet.Transform.position = x,
                                                         ShowUi,
                                                         EaseType.Out));
        });
    }
    
    // UI Helpers
    
    private void ShowUi()
    {
        ShowPlanetNavigation();
        ShowPlanetName();
        ShowPlanetControls();
    }
    private void ShowPlanetNavigation()
    {
        UI.rootVisualElement.Query("Left").First().RemoveFromClassList("Hidden");
        UI.rootVisualElement.Query("Right").First().RemoveFromClassList("Hidden");
    }
    private void ShowPlanetName()
    {
        UI.rootVisualElement.Query("Header").First().RemoveFromClassList("Hidden");
    }
    private void ShowPlanetControls()
    {
        UI.rootVisualElement.Query("Footer").First().RemoveFromClassList("Hidden");
    }
    
    private void HideUi()
    {
        HidePlanetNavigation();
        HidePlanetName();
        HidePlanetControls();
    }
    private void HidePlanetNavigation()
    {
        UI.rootVisualElement.Query("Left").First().AddToClassList("Hidden");
        UI.rootVisualElement.Query("Right").First().AddToClassList("Hidden");
    }
    private void HidePlanetName()
    {
        UI.rootVisualElement.Query("Header").First().AddToClassList("Hidden");
    }
    private void HidePlanetControls()
    {
        UI.rootVisualElement.Query("Footer").First().AddToClassList("Hidden");
    }
}
