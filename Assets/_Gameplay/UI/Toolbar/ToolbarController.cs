using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class ToolbarController : Singleton<ToolbarController>
{
    [Serializable]
    public struct System
    {
        [FormerlySerializedAs("SystemUiName")] public string UiName;
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
    }
    
    [Serializable]
    public struct Tool
    {
        [FormerlySerializedAs("ToolUiName")] public string UiName;
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
    }

    public UIDocument UI;
    
    [Header("Systems")]
    public System Global;
    public System Land;
    public System Water;
    public System Plants;

    [Header("Global Tools")]
    public Tool GlobalCamera;
    
    [Header("Land Tools")]
    public Tool MovePlate;
    public Tool BreakPlate;
    public Tool MergePlate;
    public Tool LandscapeCamera;
    
    [Header("Water Tools")]
    public Tool CloudsTool;
    public Tool WindTool;
    public Tool WaterCamera;
    
    [Header("Plants Tools")]
    public Tool NewPlantTool;
    public Tool EditPlantTool;
    public Tool ObservationCamera;
    
    private VisualElement _toolbar;
    private System? _activeSystem;
    private Tool? _activeTool;
    
    private void Start()
    {
        _toolbar = UI.rootVisualElement.Query("ToolbarContainer");
        AddButtonAction(Global.UiName, SelectGlobalSystem);
        AddButtonAction(Land.UiName, SelectLandSystem);
        AddButtonAction(Water.UiName, SelectWaterSystem);
        AddButtonAction(Plants.UiName, SelectPlantsSystem);
        AddButtonAction(MovePlate.UiName, SelectMovePlateTool);
        AddButtonAction(BreakPlate.UiName, SelectBreakPlateTool);
        AddButtonAction(MergePlate.UiName, SelectMergePlateTool);
        AddButtonAction(LandscapeCamera.UiName, SelectLandscapeCamera);
        AddButtonAction(CloudsTool.UiName, SelectCloudsTool);
        AddButtonAction(WindTool.UiName, SelectWindTool);
        AddButtonAction(WaterCamera.UiName, SelectWaterCamera);
        AddButtonAction(NewPlantTool.UiName, SelectNewPlantTool);
        AddButtonAction(EditPlantTool.UiName, SelectEditPlantTool);
        AddButtonAction(ObservationCamera.UiName, SelectObservationCamera);

        void AddButtonAction(string buttonName, Action action)
            => _toolbar.Query(buttonName).First().Query<Button>(classes: "Button").First().clicked += action;
    }

    // ToolBar
    public static void EnableToolbar()
    {
        Instance._toolbar.RemoveFromClassList("Hidden");
        SelectGlobalSystem();
    }
    public static void DisableToolbar()
    {
        Instance._toolbar.AddToClassList("Hidden");
        Instance._toolbar.RemoveFromClassList("AutoHide");
        if (Instance._activeSystem.HasValue)
        {
            Instance._toolbar.Query(name: Instance._activeSystem.Value.UiName).First().RemoveFromClassList("Active");
            Instance._activeSystem.Value.OnDeactivate.Invoke();
            Instance._activeSystem = null;
        }
        if (Instance._activeTool.HasValue)
        {
            Instance._toolbar.Query(name: Instance._activeTool.Value.UiName).First().RemoveFromClassList("Active");
            Instance._activeTool.Value.OnDeactivate.Invoke();
            Instance._activeTool = null;
        }
    }

    // Systems
    public static void SelectGlobalSystem() => Instance.ActivateSystem(Instance.Global, Instance.GlobalCamera);
    public static void SelectLandSystem() => Instance.ActivateSystem(Instance.Land, Instance.MovePlate);
    public static void SelectWaterSystem() => Instance.ActivateSystem(Instance.Water, Instance.CloudsTool);
    public static void SelectPlantsSystem() => Instance.ActivateSystem(Instance.Plants, Instance.NewPlantTool);

    // Land Tools 
    public static void SelectMovePlateTool() => Instance.ActivateTool(Instance.Land, Instance.MovePlate);
    public static void SelectBreakPlateTool() => Instance.ActivateTool(Instance.Land, Instance.BreakPlate);
    public static void SelectMergePlateTool() => Instance.ActivateTool(Instance.Land, Instance.MergePlate);
    public static void SelectLandscapeCamera() => Instance.ActivateTool(Instance.Land, Instance.LandscapeCamera);
    
    // Water Tools 
    public static void SelectCloudsTool() => Instance.ActivateTool(Instance.Water, Instance.CloudsTool);
    public static void SelectWindTool() => Instance.ActivateTool(Instance.Water, Instance.WindTool);
    public static void SelectWaterCamera() => Instance.ActivateTool(Instance.Water, Instance.WaterCamera);
    
    // Plant Tools 
    public static void SelectNewPlantTool() => Instance.ActivateTool(Instance.Plants, Instance.NewPlantTool);
    public static void SelectEditPlantTool() => Instance.ActivateTool(Instance.Plants, Instance.EditPlantTool);
    public static void SelectObservationCamera() => Instance.ActivateTool(Instance.Plants, Instance.ObservationCamera);


    // Helpers
    private void ActivateSystem(System system, Tool tool)
    {
        if (system.UiName == _activeSystem?.UiName) 
            return;

        if (_activeSystem.HasValue)
        {
            _toolbar.Query(name: _activeSystem.Value.UiName).First().RemoveFromClassList("Active");
            _toolbar.Query(name: _activeSystem.Value.UiName + "Tray").ForEach(x => x.AddToClassList("Hidden"));
            _activeSystem.Value.OnDeactivate.Invoke();
            
        }
        
        _toolbar.Query(name: system.UiName).First().AddToClassList("Active");
        _toolbar.Query(name: system.UiName + "Tray").ForEach(x => x.RemoveFromClassList("Hidden"));
        system.OnActivate.Invoke();
        _activeSystem = system;
        
        ActivateTool(system, tool);
    }
    private void ActivateTool(System system, Tool tool)
    {
        if (system.UiName != _activeSystem?.UiName)
        {
            ActivateSystem(system, tool);
        }
        else
        {
            if (tool.UiName == _activeTool?.UiName) 
                return;
        
            if (_activeTool.HasValue)
            {
                _toolbar.Query(name: _activeTool.Value.UiName).First().RemoveFromClassList("Active");
                _activeTool.Value.OnDeactivate.Invoke();
            }
            
            _toolbar.Query(name: tool.UiName).First().AddToClassList("Active");
            tool.OnActivate.Invoke();
            _activeTool = tool;
        }
    }
}
