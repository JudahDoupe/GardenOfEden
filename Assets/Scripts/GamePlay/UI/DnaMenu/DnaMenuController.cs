using Assets.Scripts.Plants.Dna;
using Stateless;
using System;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class DnaMenuController : MonoBehaviour
{
    public GameObject OpenMenuButton;
    public Panel GeneCategoryPanel;
    public Panel GeneTypePanel;
    public Panel GenePanel;
    public Panel GeneDescriptionPanel;
    public float OpenSpeed = 1f;

    public StateMachine<UiState, UiTrigger> StateMachine { get; private set; }

    private UiState _state = UiState.Closed;
    private Entity _focusedPlant;
    private Bounds _focusedBounds;
    private Dna _dna;
    private GeneCategory _category;
    private Gene _currentGene;
    private Gene _potentialGene;

    public void Enable() => StateMachine.Fire(UiTrigger.Enable);
    public void Disable() => StateMachine.Fire(UiTrigger.Disable);
    public void EditDna() => StateMachine.Fire(UiTrigger.EditDna);

    private void Start()
    {
        StateMachine = new StateMachine<UiState, UiTrigger>(() => _state, s => _state = s);

        StateMachine.Configure(UiState.Disabled)
            .OnActivate(() =>
            {
                OpenMenuButton.SetActive(false);
            })
            .Permit(UiTrigger.Enable, UiState.Closed);

        StateMachine.Configure(UiState.Closed)
            .OnEntry(() => { 
                Singleton.CameraController.LockRotation = false;
                Singleton.CameraController.LockMovement = false;
                OpenMenuButton.SetActive(true);
            })
            .OnExit(() => {
                Singleton.CameraController.LockRotation = true;
                Singleton.CameraController.LockMovement = true;
                OpenMenuButton.SetActive(false);
            })
            .Permit(UiTrigger.EditDna, UiState.CategorySelection);

        StateMachine.Configure(UiState.CategorySelection)
            .IgnoreIf(UiTrigger.EditDna, () => _focusedPlant == Entity.Null)
            .OnEntryFrom(UiTrigger.EditDna, () =>
            {
                var dnaReference = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<DnaReference>(_focusedPlant);
                _dna = DnaService.GetSpeciesDna(dnaReference.SpeciesId);

                GeneCategoryPanel.IsActive = true;
                ResetPanel(GeneCategoryPanel.Transform);
                var template = GeneCategoryPanel.Transform.Find("Template");

                foreach (var category in _dna.GetGeneCategories())
                {
                    if (!_dna.GetGeneTypes(category).Select(type => _dna.GetGene(category, type)).SelectMany(gene => DnaService.GeneLibrary.GetEvolutions(gene.Name)).Any()) continue;

                    var element = Instantiate(template, GeneCategoryPanel.Transform);
                    element.name = "TMP";
                    element.gameObject.SetActive(true);
                    element.Find("Text").gameObject.GetComponent<Text>().text = category.GetDescription();
                    element.gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                    {
                        _category = category;
                        StateMachine.Fire(UiTrigger.SelectCategory);
                    });
                }
            })
            .OnExit(() =>
            {
                GeneCategoryPanel.IsActive = false;
            })
            .Permit(UiTrigger.SelectCategory, UiState.TypeSelection)
            .Permit(UiTrigger.Close, UiState.Closed);

        StateMachine.Configure(UiState.TypeSelection)
            .OnEntryFrom(UiTrigger.SelectCategory, () =>
            {
                GeneCategoryPanel.IsActive = true;
                GeneTypePanel.IsActive = true;
                ResetPanel(GeneTypePanel.Transform);
                var template = GeneTypePanel.Transform.Find("Template");

                foreach (var type in _dna.GetGeneTypes(_category))
                {
                    var gene = _dna.GetGene(_category, type);
                    if (!DnaService.GeneLibrary.GetEvolutions(gene.Name).Any()) continue;

                    var element = Instantiate(template, GeneTypePanel.Transform);
                    element.name = "TMP";
                    element.gameObject.SetActive(true);
                    element.Find("Title").GetComponent<Text>().text = type.GetDescription();
                    element.Find("Button").Find("Text").GetComponent<Text>().text = gene.Name;
                    element.Find("Button").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                    {
                        _currentGene = gene;
                        StateMachine.Fire(UiTrigger.ViewEvolutions);
                    });
                }
            })
            .OnExit(() =>
            {
                GeneCategoryPanel.IsActive = false;
                GeneTypePanel.IsActive = false;
            })
            .PermitReentry(UiTrigger.SelectCategory)
            .Permit(UiTrigger.ViewEvolutions, UiState.Evolutions);

        StateMachine.Configure(UiState.Evolutions)
            .OnEntry(() =>
            {
                GeneTypePanel.IsActive = true;
                GenePanel.IsActive = true;
                ResetPanel(GenePanel.Transform);
                var template = GenePanel.Transform.Find("Template");

                foreach (var evolutionGene in DnaService.GeneLibrary.GetEvolutions(_currentGene.Name))
                {
                    var element = Instantiate(template, GenePanel.Transform);
                    element.name = "TMP";
                    element.gameObject.SetActive(true);
                    element.Find("Text").GetComponent<Text>().text = evolutionGene.Name;
                    element.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                    {
                        _potentialGene = evolutionGene;
                        StateMachine.Fire(UiTrigger.ViewGene);
                    });
                }
            })
            .OnExit(() =>
            {
                GeneTypePanel.IsActive = false;
                GenePanel.IsActive = false;
            })
            .PermitReentry(UiTrigger.ViewEvolutions)
            .Permit(UiTrigger.ViewGene, UiState.Description);

        StateMachine.Configure(UiState.Description)
            .OnEntry(() =>
            {
                GenePanel.IsActive = true;
                GeneDescriptionPanel.IsActive = true;

                GeneDescriptionPanel.Transform.Find("Title").gameObject.GetComponent<Text>().text = _potentialGene.Name;
                GeneDescriptionPanel.Transform.Find("DescriptionContainer").Find("Description").gameObject.GetComponent<Text>().text = _potentialGene.Description;
            })
            .OnExit(() =>
            {
                GenePanel.IsActive = false;
                GeneDescriptionPanel.IsActive = false;
            })
            .PermitReentry(UiTrigger.ViewGene)
            .Permit(UiTrigger.ViewEvolutions, UiState.Evolutions)
            .Permit(UiTrigger.Evolve, UiState.Closed);

    }

    private void Update()
    {
        if (StateMachine.IsInState(UiState.Closed))
        {
            _focusedPlant = CameraUtils.GetClosestEntity(Singleton.CameraController.FocusPos);
            if (_focusedPlant != Entity.Null)
            {
                _focusedBounds = CameraUtils.EncapsulateChildren(_focusedPlant);
                var direction = Vector3.Normalize(_focusedBounds.center);
                OpenMenuButton.transform.position = _focusedBounds.ClosestPoint(2 * _focusedBounds.center) + direction;
            }
        }

        if (!StateMachine.IsInState(UiState.Closed))
        {
            DriftCamera();
        }

        UpdatePanel(GeneCategoryPanel);
        UpdatePanel(GeneTypePanel);
        UpdatePanel(GenePanel);
        UpdatePanel(GeneDescriptionPanel);
    }

    private void DriftCamera()
    {
        var distance = Mathf.Min(CameraUtils.GetDistanceToIncludeBounds(_focusedBounds, 2.5f), 10);
        Singleton.CameraController.Rotate(new Vector3(distance / 100000, 0));
        Singleton.CameraController.MoveTo(new Coordinate(_focusedBounds.center));
        Singleton.CameraController.Zoom(distance);
    }

    private void UpdatePanel(Panel panel)
    {
        panel.Transform.localScale = Vector3.Lerp(panel.Transform.localScale, new Vector3(panel.IsActive ? 1 : 0, 1, 1), Time.deltaTime * OpenSpeed);
        panel.Transform.gameObject.SetActive(panel.Transform.localScale.x > 0.01f);
    }

    private void ResetPanel(Transform panel)
    {
        for (var i = 0; i < panel.childCount; i++)
        {
            var obj = panel.GetChild(i).gameObject;
            if (obj.name == "TMP")
            {
                Destroy(obj);
            }
        }
        panel.Find("Template").gameObject.SetActive(false);
    }

    [Serializable]
    public enum UiState
    {
        Disabled,
        Closed,
        CategorySelection,
        TypeSelection,
        Evolutions,
        Description,
    }
    [Serializable]
    public enum UiTrigger
    {
        Enable,
        Disable,
        EditDna,
        Close,
        SelectCategory,
        ViewEvolutions,
        ViewGene,
        Evolve,
    }

    [Serializable]
    public struct Panel
    {
        public Transform Transform;
        public bool IsActive;
    }
}
