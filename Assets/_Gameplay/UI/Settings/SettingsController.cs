using System;
using UnityEngine.UIElements;

public class SettingsController : Singleton<SettingsController>
{
    public UIDocument UI;

    private VisualElement _activeSettings;
    
    void Start()
    {
        AddButtonAction("Done", HideSettingsMenu);
        RegisterTabButton("Gameplay");
        RegisterTabButton("Graphics");
        RegisterTabButton("Sound");
        RegisterTabButton("Controls");

        void RegisterTabButton(string tabName) => AddButtonAction(tabName, () => SelectTab(tabName));
        void AddButtonAction(string buttonName, Action action) => UI.rootVisualElement.Query<Button>(buttonName).First().clicked += action;
    }

    
    public static void ShowSettingsMenu()
    {
        Instance.UI.sortingOrder = 1;
        Instance.UI.rootVisualElement.Query("SettingsContainer").First().RemoveFromClassList("Hidden");
        Instance.SelectTab("Gameplay");
    }
    
    public static void HideSettingsMenu()
    {
        Instance.UI.sortingOrder = 0;
        Instance.UI.rootVisualElement.Query("SettingsContainer").First().AddToClassList("Hidden");
        MainMenuController.EnableMainMenu();
    }

    
    private void SelectTab(string tab)
    {
        _activeSettings?.AddToClassList("Hidden");
        _activeSettings = UI.rootVisualElement.Query(tab + "Settings").First();
        _activeSettings.RemoveFromClassList("Hidden");
    }
}
