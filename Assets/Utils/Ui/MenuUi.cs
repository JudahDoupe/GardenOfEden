using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public class MenuUi : MonoBehaviour
    {

        public bool IsActive = false;

        public void SlideToPosition(float xPosition)
        {
            var bar = transform.GetComponent<RectTransform>();
            StartCoroutine(AnimationUtils.AnimateFloat(0.3f, bar.anchoredPosition.x, xPosition, x => bar.anchoredPosition = new Vector2(x, 0)));
        }

        public void SetButtonActive(string buttonName, bool isActive)
        {
            foreach (var button in GetComponentsInChildren<Button>().Where(x => x.transform.name == buttonName))
            {
                button.colors = isActive
                    ? ActivateButtonColor(button)
                    : DeactivateButtonColor(button);
            }
        }
        public void SetAllButtonsActive(bool isActive)
        {
            foreach (var button in GetComponentsInChildren<Button>())
            {
                button.colors = isActive
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

    public class PauseButton : IState
    {
        public PlateTectonicsToolbar Bar;
        public string ButtonName;
        public SimulationType Simulation;

        public PauseButton(PlateTectonicsToolbar bar, SimulationType simulation, string buttonName)
        {
            Bar = bar;
            ButtonName = buttonName;
            Simulation = simulation;
        }
        public void Enable()
        {
            Bar.SetButtonActive(ButtonName, true);
            SimulationController.StopSimulations(Simulation);
        }
        public void Disable()
        {
            Bar.SetButtonActive(ButtonName, false);
            SimulationController.StartSimulations(Simulation);
        }
    }

    public class ToolButton : IState
    {
        private readonly PlateTectonicsToolbar Bar;
        private readonly ITool Tool;
        private readonly string ButtonName;

        public ToolButton(PlateTectonicsToolbar bar, ITool tool, string buttonName)
        {
            Bar = bar;
            Tool = tool;
            ButtonName = buttonName;
        }

        public void Disable()
        {
            Bar.SetButtonActive(ButtonName, false);
            Tool.IsActive = false;
        }
        public void Enable()
        {
            Bar.SetButtonActive(ButtonName, true);
            Tool.IsActive = true;
        }
    }
}
