using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class SystemsMenuUi : MonoBehaviour
{
    private void Start()
    {
        Globe();
    }

    public void Enable()
    {
        var bar = transform.Find("Bar").GetComponent<RectTransform>();
        StartCoroutine(AnimationUtils.AnimateFloat(0.3f, bar.anchoredPosition.x, 0, x => bar.anchoredPosition = new Vector2(x,0)));
        FindObjectOfType<MainMenuUi>().Disable();
        Globe();
    }
    public void Disable()
    {
        var bar = transform.Find("Bar").GetComponent<RectTransform>();
        StartCoroutine(AnimationUtils.AnimateFloat(0.3f, bar.anchoredPosition.x, -70, x => bar.anchoredPosition = new Vector2(x, 0)));
    }

    public void Globe()
    {
        SetButtonActive("Globe");
        SimulationController.SetActiveSimulations(SimulationType.Water);
    }
    public void Land()
    {
        SetButtonActive("Land");
        SimulationController.SetActiveSimulations(SimulationType.PlateTectonics, SimulationType.Water);
    }

    private void SetButtonActive(string buttonName)
    {
        foreach (var button in GetComponentsInChildren<Button>())
        {
            button.colors = button.transform.name == buttonName 
                ? ActivateButtonColor(button) 
                : DeactivateButtonColor(button);
        }
    }
    private ColorBlock DeactivateButtonColor(Button button)
    {
        var colors = button.colors;
        var color = colors.highlightedColor;
        color.a = 0.5f;
        colors.highlightedColor = color;
        colors.normalColor = new Color(1, 1, 1, 0.25f);
        colors.selectedColor = new Color(1, 1, 1, 0.25f);
        colors.pressedColor = new Color(1, 1, 1, 0.25f);
        return colors;
    }
    private ColorBlock ActivateButtonColor(Button button)
    {
        var colors = button.colors;
        var color = colors.highlightedColor;
        color.a = 1;
        colors.highlightedColor = color;
        colors.normalColor = color;
        colors.selectedColor = color;
        colors.pressedColor = color;
        return colors;
    }
}
