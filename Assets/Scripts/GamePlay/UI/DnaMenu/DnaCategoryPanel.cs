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
    private Dictionary<GeneType, GameObject> _typeButtons = new Dictionary<GeneType, GameObject>();
    private Dictionary<GeneType, List<GameObject>> _evolutionButtons = new Dictionary<GeneType, List<GameObject>>();
    private Dictionary<string, GameObject> _descriptionPanels = new Dictionary<string, GameObject>();
    private UiState _state = UiState.Open;

    private StateMachine<UiState, UiTrigger>.TriggerWithParameters<GeneType> _showEvolutionTrigger;
    private StateMachine<UiState, UiTrigger>.TriggerWithParameters<string> _showDescriptionTrigger;

    public void Activate() => _stateMachine.Fire(UiTrigger.Enable);
    public void Deactivate() => _stateMachine.Fire(UiTrigger.Disable);

    public void Init(Dna dna, GeneCategory category)
    {
        ClearPanel();
        transform.localScale = Vector3.zero;

        Category = category;
        _stateMachine = new StateMachine<UiState, UiTrigger>(() => _state, s => _state = s);

        _showEvolutionTrigger = _stateMachine.SetTriggerParameters<GeneType>(UiTrigger.ShowEvolutions);
        _showDescriptionTrigger = _stateMachine.SetTriggerParameters<string>(UiTrigger.ShowDescription);

        _stateMachine.Configure(UiState.Disabled)
            .Permit(UiTrigger.Enable, UiState.Open);

        _stateMachine.Configure(UiState.Open)
            .OnEntry(EnablePanel)
            .OnExit(DisablePanel)
            .Ignore(UiTrigger.Enable)
            .Ignore(UiTrigger.HideDescription)
            .PermitReentry(UiTrigger.HideEvolutions)
            .Permit(UiTrigger.ShowEvolutions, UiState.Evolutions)
            .Permit(UiTrigger.Disable, UiState.Disabled);

        _stateMachine.Configure(UiState.Evolutions)
            .SubstateOf(UiState.Open)
            .OnEntryFrom(_showEvolutionTrigger, ShowEvolutions)
            .OnExit(HideEvolutions)
            .PermitReentry(UiTrigger.ShowEvolutions)
            .PermitReentry(UiTrigger.HideDescription)
            .Permit(UiTrigger.HideEvolutions, UiState.Open)
            .Permit(UiTrigger.ShowDescription, UiState.Description)
            .Permit(UiTrigger.Disable, UiState.Disabled);

        _stateMachine.Configure(UiState.Description)
            .SubstateOf(UiState.Evolutions)
            .OnEntryFrom(_showDescriptionTrigger, ShowDescription)
            .OnExit(HideDescription)
            .PermitReentry(UiTrigger.ShowDescription)
            .Permit(UiTrigger.HideEvolutions, UiState.Open)
            .Permit(UiTrigger.ShowEvolutions, UiState.Evolutions)
            .Permit(UiTrigger.HideDescription, UiState.Evolutions)
            .Permit(UiTrigger.Disable, UiState.Disabled);


        transform.Find("Title").GetComponent<Text>().text = category.GetDescription();
        foreach (var type in dna.GetGeneTypes(category))
        {
            var gene = dna.GetGene(category, type);
            var typeButton = Instantiate(TitledButtonPrefab, transform);
            _typeButtons[gene.Type] = typeButton;
            typeButton.transform.Find("Title").GetComponent<Text>().text = type.GetDescription();
            typeButton.transform.Find("Text").GetComponent<Text>().text = gene.Name;
            typeButton.GetComponent<Toggle>().onValueChanged.AddListener((newValue) =>
            {
                if (newValue)
                    _stateMachine.Fire(_showEvolutionTrigger, type);
                else
                    _stateMachine.Fire(UiTrigger.HideEvolutions);
            });

            var evolutions = DnaService.GeneLibrary.GetEvolutions(gene.Name);
            if (evolutions.Any())
            {
                _evolutionButtons[type] = new List<GameObject>();
                foreach (var evolution in evolutions)
                {
                    var evolutionButton = Instantiate(ButtonPrefab, typeButton.transform);
                    _evolutionButtons[type].Add(evolutionButton);
                    evolutionButton.transform.position = Vector3.zero;
                    evolutionButton.transform.localScale = Vector3.zero;
                    evolutionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);
                    evolutionButton.transform.Find("Text").GetComponent<Text>().text = evolution.Name;
                    evolutionButton.transform.localScale = Vector3.zero;
                    evolutionButton.GetComponent<Toggle>().onValueChanged.AddListener((newValue) =>
                    {
                        if (newValue)
                            _stateMachine.Fire(_showDescriptionTrigger, evolution.Name);
                        else
                            _stateMachine.Fire(UiTrigger.HideDescription);
                    });
                    evolutionButton.SetActive(false);

                    var description = Instantiate(DescriptionPrefab, evolutionButton.transform);
                    _descriptionPanels[evolution.Name] = description;
                    description.transform.position = Vector3.zero;
                    description.transform.localScale = Vector3.zero;
                    description.transform.Find("Title").GetComponent<Text>().text = evolution.Name;
                    description.transform.Find("DescriptionContainer").Find("Description").GetComponent<Text>().text = evolution.Description;
                    description.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);
                    description.transform.localScale = Vector3.zero;
                    description.SetActive(false);
                }
            }
            else
            {
                typeButton.GetComponent<Toggle>().interactable = false;
            }
        }
    }

    private void DisablePanel()
    {
        foreach (var toggle in transform.GetComponentsInChildren<Toggle>())
        {
            toggle.isOn = false;
            toggle.interactable = false;
        }
        this.AnimateUiOpacity(0.3f, 0.5f);
    }
    private void EnablePanel()
    {
        foreach (var toggle in transform.GetComponentsInChildren<Toggle>())
        {
            var title = toggle.transform.Find("Title");
            if (title != null && DnaService.GeneLibrary.GetEvolutions(title.GetComponent<Text>().text).Any())
            {
                toggle.interactable = true;
            }
        }
        this.AnimateUiOpacity(0.3f, 1);
    }

    private void ShowEvolutions(GeneType type)
    {
        transform.parent.GetComponentInParent<DnaMenuController>().SelectCategory(Category);

        var positions = new Stack<Vector3>();
        var numEvolutions = _evolutionButtons[type].Count();
        for (int i = 0; i < numEvolutions; i++)
        {
            var offset = (i - (numEvolutions - 1) / 2f) * 75;
            offset -= _typeButtons[type].transform.localPosition.y;
            positions.Push(new Vector3(300, offset));
        }

        foreach (var toggle in _evolutionButtons[type])
        {
            toggle.SetActive(true);
            toggle.GetComponent<Toggle>().AnimateTransform(0.3f, positions.Pop(), Vector3.one);
        }
    }
    private void HideEvolutions()
    {
        foreach (var button in _evolutionButtons.Values.SelectMany(x => x))  
        {
            button.GetComponent<Toggle>().isOn = false;
            button.GetComponent<Image>().AnimateTransform(0.3f, Vector3.zero, Vector3.zero, false);
        }
    }

    private void ShowDescription(string evolution)
    {
        var panel = _descriptionPanels[evolution];
        var offset = -panel.transform.parent.localPosition.y - panel.transform.parent.parent.localPosition.y;
        panel.SetActive(true);
        panel.GetComponent<Image>().AnimateTransform(0.3f, new Vector3(300, offset), Vector3.one);
    }
    private void HideDescription()
    {
        foreach (var panel in _descriptionPanels.Values)
        {
            panel.GetComponent<Image>().AnimateTransform(0.3f, Vector3.zero, Vector3.zero, false);
        }
    }

    private void ClearPanel()
    {
        DestroyAll(_typeButtons.Values);
        _typeButtons.Clear();
        DestroyAll(_evolutionButtons.Values.SelectMany(x => x));
        _evolutionButtons.Clear();
        DestroyAll(_descriptionPanels.Values);
        _descriptionPanels.Clear();
    }
    private void DestroyAll(IEnumerable<GameObject> objs)
    {
        foreach (var o in objs)
        {
            Destroy(o);
        }
    }


    public enum UiState
    {
        Disabled,
        Open,
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
