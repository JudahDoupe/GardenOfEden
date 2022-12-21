using Assets.GamePlay.Cameras;
using UnityEngine;

public class LandscapeCameraTool : MonoBehaviour, ITool
{
    public bool IsActive { get; private set; }
    public void Enable()
    {
        IsActive = true;
        CameraController.TransitionToLandscapeCamera(CameraTransition.SmoothFast);
        InputAdapter.Cancel.Subscribe(this, ToolbarController.SelectMovePlateTool);
    }

    public void Disable()
    {
        IsActive = false;
        InputAdapter.Cancel.Unsubscribe(this);
    }
}
