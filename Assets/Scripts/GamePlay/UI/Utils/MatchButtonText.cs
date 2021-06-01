using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MatchButtonText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;

    public void Start()
    {
        button = GetComponent<Button>();
        GetComponentInChildren<Text>().color = GetColor(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponentInChildren<Text>().color = GetColor(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponentInChildren<Text>().color = GetColor(false);
    }

    private Color GetColor(bool isSelected)
    {
        return button.IsInteractable()
            ? (isSelected ? button.colors.highlightedColor : button.colors.normalColor)
            : button.colors.disabledColor;
    }
}
