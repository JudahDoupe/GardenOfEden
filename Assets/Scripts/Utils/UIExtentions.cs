using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIExtentions
{
    public static void RegisterClickedAction(this Button element, Action action)
    {
        if (element != null)
        {
            element.clickable.clicked += action;
        }
        else
        {
            Debug.Log($"Could not find {element}");
        }
    }
}
