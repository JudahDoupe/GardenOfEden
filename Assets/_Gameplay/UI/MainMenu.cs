using Assets.GamePlay.Cameras;
using Assets.Scripts.Utils;
using UnityEngine;

public class MainMenu : MenuUi
{
    public override void Enable()
    {
        var home = transform.Find("Home");
        home.gameObject.SetActive(true);
        home.AnimatePosition(0.3f, new Vector3(350, 0, 0));
        Singleton.PerspectiveController.SetPerspective(FindObjectOfType<MainMenuCamera>(), CameraTransition.Smooth);
        SimulationController.StartSimulations(SimulationType.Water);
    }
    public override void Disable()
    {
        var home = transform.Find("Home");
        home.AnimatePosition(0.3f, new Vector3(-350, 0, 0), () => home.gameObject.SetActive(false));
        SimulationController.StopSimulations(SimulationType.Water);
    }

    public void Continue()
    {
        Disable();
        FindObjectOfType<SystemsMenu>().Enable();
    }
    public void Quit() => Application.Quit();
}
