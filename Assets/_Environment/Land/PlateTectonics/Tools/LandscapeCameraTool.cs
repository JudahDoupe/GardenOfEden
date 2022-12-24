using Assets.GamePlay.Cameras;
using UnityEngine;

public class LandscapeCameraTool : MonoBehaviour, ITool
{
    private PlateTectonicsData _data;

    private void Start() => Planet.Data.Subscribe(data => _data = data.PlateTectonics);

    public bool IsActive { get; private set; }

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