using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugMenuController : MonoBehaviour
{
    public UIDocument UI;

    private List<float> _fps = new();
    private bool _isActive;
    
    private void Start()
    {
        InputAdapter.Debug.Subscribe(this, () =>
        {
            UI.rootVisualElement.Q("DebugContainer").ToggleInClassList("Hidden");
            _isActive = !UI.rootVisualElement.Q("DebugContainer").ClassListContains("Hidden");
        });
    }

    void Update()
    {
        if (!_isActive) return;
        
        _fps.Add(1.0f / Time.deltaTime);
        if (_fps.Count > 120) _fps.RemoveAt(0);

        UpdateFPS();
    }

    
    
    private void UpdateFPS()
    {
        UI.rootVisualElement.Q<Label>("FPS").text = $"{_fps.Average()} FPS";
    }
}
