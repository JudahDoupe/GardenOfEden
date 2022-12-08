using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarController : MonoBehaviour
{
    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            if (_isActive)
            {
                _toolbar.RemoveFromClassList("Hidden");
                SelectGlobe();
            }
            else
            {
                _toolbar.AddToClassList("Hidden");
            }
        }
    }

    private VisualElement _toolbar;
    private VisualElement _globe => _toolbar.Query(name: "Globe");
    private VisualElement _landSystem => _toolbar.Query(name: "LandSystem");
    private VisualElement _movePlateTool => _toolbar.Query(name: "MovePlateTool");
    private VisualElement _breakPlateTool => _toolbar.Query(name: "BreakPlateTool");
    private VisualElement _mergePlateTool => _toolbar.Query(name: "MergePlateTool");
    private VisualElement _landscapeCamera => _toolbar.Query(name: "LandscapeCamera");
    

    private VisualElement _activeSystem;
    private VisualElement _activeTool;
    
    private void Start()
    {
        _toolbar = GetComponent<UIDocument>().rootVisualElement;
        _globe.Query<Button>(classes: "Button").First().clicked += () => ActivateSystem(_globe);
        _landSystem.Query<Button>(classes: "Button").First().clicked += () =>
        {
            ActivateSystem(_landSystem);
            ActivateTool(_movePlateTool);
        };
    }
    
    public void SelectGlobe()
    {
        _activeSystem = _globe;
    }

    public void SelectLandSystem()
    {
        _activeSystem = _landSystem;
        SelectMovePlateTool();
    }

    public void SelectMovePlateTool()
    {
        _activeTool = _movePlateTool;
    }
    
    private void ActivateSystem(VisualElement systemUi)
    {
        _activeSystem?.RemoveFromClassList("Active");
        
        _activeSystem = systemUi;
        _activeSystem.AddToClassList("Active");
    }

    private void ActivateTool(VisualElement toolUi)
    {
        _activeTool?.RemoveFromClassList("Active");
        
        _activeTool = toolUi;
        _activeTool.AddToClassList("Active");
    }
}
