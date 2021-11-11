using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class SystemsMenu : MenuUi
{
    private void Start()
    {
        Globe();
    }

    public void Enable()
    {
        SetAllButtonsActive(false);
        SlideToPosition(0);
        FindObjectOfType<MainMenuUi>().Disable();
        Globe();
    }
    public void Disable()
    {
        SlideToPosition(-70);
    }

    public void Globe()
    {
        SetButtonActive("Globe", true);
        SetButtonActive("Land", false);
        SimulationController.SetActiveSimulations(SimulationType.Water);
        FindObjectOfType<PlateTectonicsToolbar>().Disable();
    }
    public void Land()
    {
        SetButtonActive("Land", true);
        SetButtonActive("Globe", false);
        SimulationController.SetActiveSimulations(SimulationType.PlateTectonics, SimulationType.Water);
        FindObjectOfType<PlateTectonicsToolbar>().Enable();
    }
}
