using System;
using UnityEngine.UIElements;

public class SettingsController : Singleton<SettingsController>
{
    public UIDocument UI;

    private VisualElement _activeSettings;
    private PlayerData _playerData;
    
    void Start()
    {
        
        this.RunTaskInCoroutine(PlayerDataStore.GetOrCreate(), () =>
        {
            _playerData = PlayerDataStore.GetOrCreate().Result;
            
            RegisterSliderSetting("ZoomSlider", _playerData.Settings.ScrollSpeed, x => _playerData.Settings.ScrollSpeed = x);
            RegisterSliderSetting("DragSlider", _playerData.Settings.DragSpeed, x => _playerData.Settings.DragSpeed = x);
        });
        
        RegisterButtonAction("Done", HideSettingsMenu);
        RegisterTabButton("Gameplay");
        RegisterTabButton("Graphics");
        RegisterTabButton("Sound");
        RegisterTabButton("Controls");

        void RegisterTabButton(string tabName) 
            => RegisterButtonAction(tabName, () => SelectTab(tabName));
        void RegisterButtonAction(string buttonName, Action action) 
            => UI.rootVisualElement.Query<Button>(buttonName).First().clicked += action;
        void RegisterSliderSetting(string sliderName, float value, Action<float> setter)
        {
            var slider = UI.rootVisualElement.Query<Slider>(sliderName).First();
            slider.value = value;
            slider.RegisterValueChangedCallback(x =>
            {
                setter(x.newValue);
                PlayerDataStore.Update(_playerData).ConfigureAwait(false);
            });
        }
    }

    
    public static void ShowSettingsMenu()
    {
        Instance.UI.rootVisualElement.Query("ScreenContainer").First().RemoveFromClassList("Hidden");
        Instance.SelectTab("Gameplay");
    }
    
    public static void HideSettingsMenu()
    {
        Instance.UI.rootVisualElement.Query("ScreenContainer").First().AddToClassList("Hidden");
        MainMenuController.EnableMainMenu();
    }

    
    private void SelectTab(string tab)
    {
        _activeSettings?.AddToClassList("Hidden");
        _activeSettings = UI.rootVisualElement.Query(tab + "Settings").First();
        _activeSettings.RemoveFromClassList("Hidden");
    }
}
