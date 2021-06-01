using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class MatchToggleText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Toggle toggle;

    public void Start()
    {
        toggle = GetComponent<Toggle>();
        GetComponentInChildren<Text>().color = GetColor(toggle.isOn);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponentInChildren<Text>().color = GetColor(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponentInChildren<Text>().color = GetColor(toggle.isOn);
    }

    private Color GetColor(bool isSelected)
    {
        return toggle.IsInteractable()
            ? (isSelected ? toggle.colors.highlightedColor : toggle.colors.normalColor)
            : toggle.colors.disabledColor;
    }
}
