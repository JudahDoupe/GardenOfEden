using Assets.GamePlay.Cameras;
using Assets.Scripts.Utils;
using UnityEngine;

public class MainMenu : MenuUi
{

    private void Start()
    {
        Enable();
    }

    public override void Enable()
    {
        var home = transform.Find("Home");
        home.gameObject.SetActive(true);
        home.AnimatePosition(0.3f, new Vector3(350, 0, 0));
        CameraController.SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Smooth);
    }
    public override void Disable()
    {
        var home = transform.Find("Home");
        home.AnimatePosition(0.3f, new Vector3(-350, 0, 0), () => home.gameObject.SetActive(false));
    }

    public void Continue()
    {
        Disable();
        ToolbarController.EnableToolbar();
    }
    public void Quit() => Application.Quit();
}
