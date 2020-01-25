using Unity.UIElements.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EvolutionUIController : MonoBehaviour
{
    private VisualElement _evolutionUi;
    void Start()
    {
        _evolutionUi = GetComponent<PanelRenderer>().visualTree;
        var leavesButton = _evolutionUi.Q<Button>("leaves");
        if (leavesButton != null)
        {
            leavesButton.clickable.clicked += () => { Debug.Log("Clicked the leaves button"); };
        }
        else
        {
            Debug.Log("Could not find leaves button");
        }
    }

}
