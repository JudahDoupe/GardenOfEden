using Assets.GamePlay.Cameras;
using Assets.Scripts.Utils;
using UnityEngine;

public class MainMenu : MenuUi
{

    private SystemsController _systemsContoller;

    private void Start()
    {
        _systemsContoller = FindObjectOfType<SystemsController>();
        Enable();
    }

    public override void Enable()
    {
        var home = transform.Find("Home");
        home.gameObject.SetActive(true);
        home.AnimatePosition(0.3f, new Vector3(350, 0, 0));
        CameraController.SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Smooth);
        _systemsContoller.EnableGlobe();
    }
    public override void Disable()
    {
        var home = transform.Find("Home");
        home.AnimatePosition(0.3f, new Vector3(-350, 0, 0), () => home.gameObject.SetActive(false));
    }

    public void Continue()
    {
        Disable();
        ToolbarController.ShowToolbar();
        ToolbarController.SelectGlobalSystem();
    }
    public void Quit() => Application.Quit();
}
