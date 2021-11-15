using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public class MenuUi : MonoBehaviour, IState
    {

        public bool IsActive = false;
        public virtual void Enable() => IsActive = true;
        public virtual void Disable() => IsActive = false;

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

    public class ButtonState : IState
    {
        private readonly MenuUi Bar;
        private readonly string ButtonName;
        private readonly Action<bool> OnSetEnable;

        public ButtonState(MenuUi bar, string buttonName)
        {
            Bar = bar;
            ButtonName = buttonName;
            OnSetEnable = enabled => { };
        }
        public ButtonState(MenuUi bar, string buttonName, Action<bool> onSetEnable)
        {
            Bar = bar;
            ButtonName = buttonName;
            OnSetEnable = onSetEnable;
        }
        public ButtonState(MenuUi bar, string buttonName, Action onEnable, Action onDisable)
        {
            Bar = bar;
            ButtonName = buttonName;
            OnSetEnable = enabled =>
            {
                if (enabled) onEnable();
                else onDisable();
            };
        }

        public void Enable()
        {
            Bar.SetButtonActive(ButtonName, true);
            OnSetEnable(true);
        }
        public void Disable()
        {
            Bar.SetButtonActive(ButtonName, false);
            OnSetEnable(false);
        }
    }
}
