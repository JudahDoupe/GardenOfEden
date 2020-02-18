using System.Collections;
using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    public enum UIState
    {
        None,
        Evolution
    }
    public UIState State = UIState.None;
    public GameObject EvolutionUI;

    private void Start()
    {
        SetState(UIState.None);
    }

    public void SetState(UIState state)
    {
        SetEnabled(EvolutionUI, false);
        switch (state)
        {
            case UIState.None:
                break;
            case UIState.Evolution:
                SetEnabled(EvolutionUI, true);
                break;
        }
    }

    private void SetEnabled(GameObject ui, bool enabled)
    {
        var panel = ui.GetComponent<PanelRenderer>();
        var eventSystem = ui.GetComponent<UIElementsEventSystem>();

        panel.visualTree.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        panel.enabled = enabled;
        eventSystem.enabled = enabled;
    }
}
