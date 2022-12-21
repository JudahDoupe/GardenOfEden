using Assets.GamePlay.Cameras;
using UnityEngine;

public class GlobalCameraTool : MonoBehaviour, ITool
{
    public bool IsActive { get; private set; }
    public void Enable()
    {
        IsActive = true;
        CameraController.TransitionToSatelliteCamera(CameraTransition.Smooth);
        InputAdapter.Cancel.Subscribe(this, () => {
            ToolbarController.DisableToolbar();
            MainMenuController.EnableMainMenu();
        });
    }

    public void Disable()
    {
        IsActive = false;
        InputAdapter.Cancel.Unsubscribe(this);
    }
}
