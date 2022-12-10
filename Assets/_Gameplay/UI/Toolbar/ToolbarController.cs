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

    [Header("Systems")]
    public System Global;
    public System Land;

    [Header("Global Tools")]
    public Tool GlobalCamera;
    
    [Header("Land Tools")]
    public Tool MovePlate;
    public Tool BreakPlate;
    public Tool MergePlate;
    public Tool LandscapeCamera;
    
    private VisualElement _toolbar;
    private System? _activeSystem;
    private Tool? _activeTool;
    
    private void Start()
    {
        _toolbar = GetComponent<UIDocument>().rootVisualElement.Query("ToolbarContainer");
        AddButtonAction(Global.UiName, SelectGlobalSystem);
        AddButtonAction(Land.UiName, SelectLandSystem);
        AddButtonAction(MovePlate.UiName, SelectMovePlateTool);
        AddButtonAction(BreakPlate.UiName, SelectBreakPlateTool);
        AddButtonAction(MergePlate.UiName, SelectMergePlateTool);
        AddButtonAction(LandscapeCamera.UiName, SelectLandscapeCamera);

        void AddButtonAction(string buttonName, Action action)
            => _toolbar.Query(buttonName).First().Query<Button>(classes: "Button").First().clicked += action;
    }

    // ToolBar
    public static void ShowToolbar()
    {
        Instance._toolbar.RemoveFromClassList("Hidden");
        Instance._toolbar.RemoveFromClassList("AutoHide");
    }
    public static void HideToolbar()
    {
        Instance._toolbar.AddToClassList("Hidden");
        Instance._toolbar.RemoveFromClassList("AutoHide");
    }
    public static void AutoHideToolbar()
    {
        Instance._toolbar.RemoveFromClassList("Hidden");
        Instance._toolbar.AddToClassList("AutoHide");
    }


    // Systems
    public static void SelectGlobalSystem() => Instance.ActivateSystem(Instance.Global, Instance.GlobalCamera);
    public static void SelectLandSystem() => Instance.ActivateSystem(Instance.Land, Instance.MovePlate);
    
    // Global Tools
    public static void SelectGlobalCamera() => Instance.ActivateTool(Instance.Land, Instance.MovePlate);
    
    // Land Tools 
    public static void SelectMovePlateTool() => Instance.ActivateTool(Instance.Land, Instance.MovePlate);
    public static void SelectBreakPlateTool() => Instance.ActivateTool(Instance.Land, Instance.BreakPlate);
    public static void SelectMergePlateTool() => Instance.ActivateTool(Instance.Land, Instance.MergePlate);
    public static void SelectLandscapeCamera() => Instance.ActivateTool(Instance.Land, Instance.LandscapeCamera);
    
    // Helpers
    private void ActivateSystem(System system, Tool tool)
    {
        if (system.UiName == _activeSystem?.UiName) 
            return;

        if (_activeSystem.HasValue)
        {
            _toolbar.Query(name: _activeSystem.Value.UiName).First().RemoveFromClassList("Active");
            _activeSystem.Value.OnDeactivate.Invoke();
        }
        
        _toolbar.Query(name: system.UiName).First().AddToClassList("Active");
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
