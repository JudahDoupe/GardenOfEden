using System;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class EvolutionUIController : MonoBehaviour
{
    private VisualElement _root;
    void OnEnabled()
    {
        _root = GetComponent<PanelRenderer>().visualTree;

        _root.Q<Button>("leaves").RegisterClickedAction(ClickLeaves);
    }
    private void ClickLeaves()
    {
        Debug.Log("You clicked the leaves");
    }
}
