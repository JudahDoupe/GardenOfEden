using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class EvolutionUIController : MonoBehaviour
{
    public GameObject EvolutionUI;
    public bool IsEnabled 
    {
        get { return EvolutionUI.activeSelf; }
        set { EvolutionUI.SetActive(value); }
    }

    private VisualElement _evolutionUi;
    void Start()
    {
        _evolutionUi = EvolutionUI.GetComponent<PanelRenderer>().visualTree;
        var leavesButton = _evolutionUi.Q<Button>("leaves");
        if (leavesButton != null)
        {
            leavesButton.clickable.clicked += () => { Debug.Log("Clicked the leaves button"); };
        }
        else
        {
            Debug.Log("Could not find leaves button");
        }

        IsEnabled = false;
    }
}
