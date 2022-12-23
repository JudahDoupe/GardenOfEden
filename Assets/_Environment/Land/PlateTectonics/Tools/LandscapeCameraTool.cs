using Assets.GamePlay.Cameras;
using UnityEngine;

public class LandscapeCameraTool : MonoBehaviour, ITool
{
    public bool IsActive { get; private set; }
    public bool IsInitialized { get; private set; }
    
    private PlateTectonicsData _data;
    
    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
        IsInitialized = true;
    }
    public void Unlock() => _data.GetTool(nameof(LandscapeCameraTool)).Unlock();
    public void Enable()
    {
        IsActive = true;
        _data.GetTool(nameof(LandscapeCameraTool)).Use();
        CameraController.TransitionToLandscapeCamera(CameraTransition.SmoothFast);
        InputAdapter.Cancel.Subscribe(this, ToolbarController.SelectMovePlateTool);
    }
    public void Disable()
    {
        IsActive = false;
        InputAdapter.Cancel.Unsubscribe(this);
    }
    
}
