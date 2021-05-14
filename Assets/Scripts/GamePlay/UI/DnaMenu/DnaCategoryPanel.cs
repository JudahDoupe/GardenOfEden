using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Dna;
using Assets.Scripts.Utils;
using Stateless;
using UnityEngine;
using UnityEngine.UI;

public class DnaCategoryPanel : MonoBehaviour
{
    public GameObject TitledButtonPrefab;
    public GameObject ButtonPrefab;
    public GameObject DescriptionPrefab;

    public GeneCategory Category;

    private StateMachine<UiState, UiTrigger> _stateMachine;
    private UiState _currentState = UiState.CurrentGenes;
    private StateMachine<UiState, UiTrigger>.TriggerWithParameters<GeneType> _showEvolutionTrigger;
    private StateMachine<UiState, UiTrigger>.TriggerWithParameters<string> _showDescriptionTrigger;

    private readonly Dictionary<GeneType, GameObject> _currentGeneButtons = new Dictionary<GeneType, GameObject>();
    private readonly Dictionary<GeneType, List<GameObject>> _evolutionButtons = new Dictionary<GeneType, List<GameObject>>();
    private readonly Dictionary<string, GameObject> _descriptionPanels = new Dictionary<string, GameObject>();


    public void Init(Dna dna, GeneCategory category)
    {
        transform.localScale = Vector3.zero;

        Category = category;
        _stateMachine = new StateMachine<UiState, UiTrigger>(() => _currentState, s => _currentState = s);

        _showEvolutionTrigger = _stateMachine.SetTriggerParameters<GeneType>(UiTrigger.ShowEvolutions);
        _showDescriptionTrigger = _stateMachine.SetTriggerParameters<string>(UiTrigger.ShowDescription);

        _stateMachine.Configure(UiState.Disabled)
            .OnEntry(DisablePanel)
            .Ignore(UiTrigger.Disable)
            .Ignore(UiTrigger.HideEvolutions)
            .Ignore(UiTrigger.HideDescription)
            .Permit(UiTrigger.Enable, UiState.CurrentGenes);

        _stateMachine.Configure(UiState.CurrentGenes)
            .OnEntry(EnablePanel)
            .Ignore(UiTrigger.Enable)
            .Ignore(UiTrigger.HideDescription)
            .PermitReentry(UiTrigger.HideEvolutions)
            .Permit(UiTrigger.ShowEvolutions, UiState.Evolutions)
            .Permit(UiTrigger.Disable, UiState.Disabled);

        _stateMachine.Configure(UiState.Evolutions)
            .SubstateOf(UiState.CurrentGenes)
            .OnEntryFrom(_showEvolutionTrigger, ShowEvolutions)
            .OnExit(HideEvolutions)
            .PermitReentry(UiTrigger.ShowEvolutions)
            .PermitReentry(UiTrigger.HideDescription)
            .Permit(UiTrigger.HideEvolutions, UiState.CurrentGenes)
            .Permit(UiTrigger.ShowDescription, UiState.Description)
            .Permit(UiTrigger.Disable, UiState.Disabled);

        _stateMachine.Configure(UiState.Description)
            .SubstateOf(UiState.Evolutions)
            .OnEntryFrom(_showDescriptionTrigger, ShowDescription)
            .OnExit(HideDescription)
            .PermitReentry(UiTrigger.ShowDescription)
            .Permit(UiTrigger.HideEvolutions, UiState.CurrentGenes)
            .Permit(UiTrigger.ShowEvolutions, UiState.Evolutions)
            .Permit(UiTrigger.HideDescription, UiState.Evolutions)
            .Permit(UiTrigger.Disable, UiState.Disabled);


        transform.Find("Title").GetComponent<Text>().text = category.GetDescription();
        foreach (var type in dna.GetGeneTypes(category))
        {
            var gene = dna.GetGene(category, type);
            var currentGeneButton = InitCurrentGene(gene);

            var evolutions = DnaService.GeneLibrary.GetEvolutions(gene.Name);
            if (evolutions.Any())
            {
                _evolutionButtons[type] = new List<GameObject>();
                foreach (var evolution in evolutions)
                {
                    var evolutionButton = InitEvolution(evolution, currentGeneButton.transform);
                    var descriptionPanel = InitDescription(evolution, evolutionButton.transform);
                }
            }
            else
            {
                currentGeneButton.GetComponent<Toggle>().interactable = false;
            }
        }
    }
    public void Activate() => _stateMachine.Fire(UiTrigger.Enable);
    public void Deactivate() => _stateMachine.Fire(UiTrigger.Disable);


    private GameObject InitCurrentGene(Gene gene)
    {
        var currentGeneButton = Instantiate(TitledButtonPrefab, transform);
        _currentGeneButtons[gene.Type] = currentGeneButton;
        currentGeneButton.transform.Find("Title").GetComponent<Text>().text = gene.Type.GetDescription();
        currentGeneButton.transform.Find("Text").GetComponent<Text>().text = gene.Name;
        currentGeneButton.GetComponent<Toggle>().onValueChanged.AddListener((newValue) =>
        {
            if (newValue)
                _stateMachine.Fire(_showEvolutionTrigger, gene.Type);
            else
                _stateMachine.Fire(UiTrigger.HideEvolutions);
        });
        return currentGeneButton;
    }
    private void DisablePanel()
    {
        foreach (var toggle in transform.GetComponentsInChildren<Toggle>())
        {
            toggle.isOn = false;
            toggle.interactable = false;
        }
    }
    private void EnablePanel()
    {
        foreach (var currentGene in _currentGeneButtons.Values)
        {
            if (DnaService.GeneLibrary.GetEvolutions(currentGene.transform.Find("Text").GetComponent<Text>().text).Any())
            {
                currentGene.GetComponent<Toggle>().interactable = true;
            }
        }
    }

    public GameObject InitEvolution(Gene gene, Transform parent)
    {
        var evolutionButton = Instantiate(ButtonPrefab, parent);
        _evolutionButtons[gene.Type].Add(evolutionButton);
        evolutionButton.transform.position = Vector3.zero;
        evolutionButton.transform.localScale = Vector3.zero;
        evolutionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);
        evolutionButton.transform.Find("Text").GetComponent<Text>().text = gene.Name;
        evolutionButton.transform.localScale = Vector3.zero;
        evolutionButton.GetComponent<Toggle>().interactable = false;
        evolutionButton.GetComponent<Toggle>().onValueChanged.AddListener((newValue) =>
        {
            if (newValue)
                _stateMachine.Fire(_showDescriptionTrigger, gene.Name);
            else
                _stateMachine.Fire(UiTrigger.HideDescription);
        });
        return evolutionButton;
    }
    private void ShowEvolutions(GeneType type)
    {
        transform.parent.GetComponentInParent<DnaMenuController>().SelectCategory(Category);

        var positions = new Stack<Vector3>();
        var numEvolutions = _evolutionButtons[type].Count();
        for (int i = 0; i < numEvolutions; i++)
        {
            var offset = (i - (numEvolutions - 1) / 2f) * 75;
            offset -= _currentGeneButtons[type].transform.localPosition.y;
            positions.Push(new Vector3(400, offset));
        }

        foreach (var toggle in _evolutionButtons[type])
        {
            toggle.GetComponent<Toggle>().interactable = true;
            toggle.transform.AnimateTransform(0.3f, positions.Pop(), Vector3.one);
        }
    }
    private void HideEvolutions()
    {
        foreach (var button in _evolutionButtons.Values.SelectMany(x => x))  
        {
            button.GetComponent<Toggle>().isOn = false;
            button.GetComponent<Toggle>().interactable = false;
            button.transform.AnimateTransform(0.3f, Vector3.zero, Vector3.zero);
        }
    }

    public GameObject InitDescription(Gene gene, Transform parent)
    {
        var descriptionPanel = Instantiate(DescriptionPrefab, parent);
        _descriptionPanels[gene.Name] = descriptionPanel;
        descriptionPanel.transform.position = Vector3.zero;
        descriptionPanel.transform.localScale = Vector3.zero;
        descriptionPanel.transform.Find("Title").GetComponent<Text>().text = gene.Name;
        descriptionPanel.transform.Find("DescriptionContainer").Find("Description").GetComponent<Text>().text = gene.Description;
        descriptionPanel.transform.Find("Button").GetComponent<Button>().onClick
            .AddListener(() => GetComponentInParent<DnaMenuController>().Evolve(gene));
        descriptionPanel.SetActive(false);
        return descriptionPanel;
    }
    private void ShowDescription(string evolution)
    {
        var panel = _descriptionPanels[evolution];
        var offset = -panel.transform.parent.localPosition.y - panel.transform.parent.parent.localPosition.y;
        panel.SetActive(true);
        panel.transform.AnimateTransform(0.3f, new Vector3(400, offset), Vector3.one);
    }
    private void HideDescription()
    {
        foreach (var panel in _descriptionPanels.Values)
        {
            panel.transform.AnimateTransform(0.3f, Vector3.zero, Vector3.zero, false);
        }
    }


    public enum UiState
    {
        Disabled,
        CurrentGenes,
        Evolutions,
        Description,
    }

    public enum UiTrigger
    {
        Enable,
        Disable,
        ShowEvolutions,
        HideEvolutions,
        ShowDescription,
        HideDescription,
    }
}
