using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class StickyToggle : MonoBehaviour
{
    private Toggle _toggle;
    private ColorBlock _onColors;
    private ColorBlock _offColors;

    void Start()
    {
        _toggle = GetComponent<Toggle>();
        _onColors = _toggle.colors;
        _offColors = _toggle.colors;

        _offColors.normalColor = _onColors.normalColor;
        _offColors.selectedColor = _onColors.normalColor;

        _onColors.normalColor = _onColors.selectedColor;
    }

    void Update()
    { 
        _toggle.colors = _toggle.isOn ? _onColors : _offColors;
    }
}
