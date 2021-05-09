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
    public GameObject GeneTypesPanel;
    public GameObject GenesPanel;
    public GameObject DescriptionPanel;
    public CategoryButton[] CategoryButtons;
    public float OpenSpeed = 1f;
    public float Distance = 3f;

    public StateMachine<UiState, UiTrigger> StateMachine { get; private set; }

    private UiState _state = UiState.Closed;
    private Entity _focusedPlant;
    private Bounds _focusedBounds;
    private Dna _dna;
    private GeneCategory _category;
    private string _currentGene;
    private string _potentialGene;

    public void Enable() => StateMachine.Fire(UiTrigger.Enable);
    public void Disable() => StateMachine.Fire(UiTrigger.Disable);
    public void EditDna() => StateMachine.Fire(UiTrigger.EditDna);
    public void SelectCategory(int category)
    {
        _category = (GeneCategory)category;
        StateMachine.Fire(UiTrigger.SelectCategory);
    }
    public void ViewEvolutions(string gene)
    {
        _currentGene = gene;
        StateMachine.Fire(UiTrigger.ViewEvolutions);
    }
    public void ViewGene(string gene)
    {
        _potentialGene = gene;
        StateMachine.Fire(UiTrigger.ViewGene);
    }
    public void Evolve()
    {

    }

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

                for (int i = 0; i < CategoryButtons.Length; i++)
                {
                    CategoryButtons[i].IsActive = _dna.GetGeneCategories.Contains(CategoryButtons[i].Category);
                }
            })
            .OnExit(() =>
            {
                for (int i = 0; i < CategoryButtons.Length; i++)
                {
                    CategoryButtons[i].IsActive = false;
                }
            })
            .Permit(UiTrigger.SelectCategory, UiState.Category)
            .Permit(UiTrigger.Close, UiState.Closed);

        StateMachine.Configure(UiState.Category)
            .OnEntryFrom(UiTrigger.SelectCategory, () =>
            {
                var template = GeneTypesPanel.transform.Find("Template");
                for (var i = 0; i < GeneTypesPanel.transform.childCount; i++)
                {
                    var obj = GeneTypesPanel.transform.GetChild(i).gameObject;
                    if (obj.name != "Template")
                    {
                        Destroy(obj);
                    }
                }

                foreach (var type in _dna.GetGeneTypes(_category))
                {
                    var geneName = _dna.GetGene(_category, type).Name;
                    var typeUi = Instantiate(template, GeneTypesPanel.transform);
                    typeUi.Find("Title").GetComponent<Text>().text = type.GetDescription();
                    typeUi.Find("Button").Find("Text").GetComponent<Text>().text = geneName;
                    typeUi.Find("Button").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ViewEvolutions(geneName));
                    typeUi.gameObject.SetActive(true);
                }

                template.gameObject.SetActive(false);
                GeneTypesPanel.SetActive(true);
            })
            .OnExit(() =>
            {
                GeneTypesPanel.SetActive(false);
            })
            .Permit(UiTrigger.ViewEvolutions, UiState.Evolutions);

        StateMachine.Configure(UiState.Evolutions)
            .OnEntry(() =>
            {
                var template = GenesPanel.transform.Find("Template");
                for (var i = 0; i < GenesPanel.transform.childCount; i++)
                {
                    var obj = GenesPanel.transform.GetChild(i).gameObject;
                    if (obj.name != "Template")
                    {
                        Destroy(obj);
                    }
                }

                foreach (var evolutionGene in DnaService.GeneLibrary.GetEvolutions(_currentGene))
                {
                    var geneButton = Instantiate(template, GenesPanel.transform);
                    geneButton.Find("Text").GetComponent<Text>().text = evolutionGene.Name;
                    geneButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ViewGene(evolutionGene.Name));
                    geneButton.gameObject.SetActive(true);
                }

                template.gameObject.SetActive(false);
                GeneTypesPanel.SetActive(true);
                GenesPanel.SetActive(true);
            })
            .OnExit(() =>
            {
                GeneTypesPanel.SetActive(false);
                GenesPanel.SetActive(false);
            })
            .PermitReentry(UiTrigger.ViewEvolutions)
            .Permit(UiTrigger.ViewGene, UiState.Description);

        StateMachine.Configure(UiState.Description)
            .OnEntry(() =>
            {
                var gene = DnaService.GeneLibrary.GetGene(_potentialGene);
                DescriptionPanel.transform.Find("Title").gameObject.GetComponent<Text>().text = gene.Name;
                DescriptionPanel.transform.Find("DescriptionContainer").Find("Description").gameObject.GetComponent<Text>().text = gene.Description;

                GeneTypesPanel.SetActive(true);
                GenesPanel.SetActive(true);
                DescriptionPanel.SetActive(true);
            })
            .OnExit(() =>
            {
                GeneTypesPanel.SetActive(false);
                GenesPanel.SetActive(false);
                DescriptionPanel.SetActive(false);
            })
            .PermitReentry(UiTrigger.ViewGene)
            .Permit(UiTrigger.ViewEvolutions, UiState.Evolutions);

    }

    private  void Update()
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

        var lastIndex = -1;
        for (int i = 0; i < CategoryButtons.Length; i++)
        {
            var index = CategoryButtons[i].IsActive ? ++lastIndex : -1;
            UpdateCategoryButton(CategoryButtons[i].Button, index, CategoryButtons[i].IsActive);
        }
    }

    private void DriftCamera()
    {
        var distance = Mathf.Min(CameraUtils.GetDistanceToIncludeBounds(_focusedBounds, 2.5f), 10);
        Singleton.CameraController.Rotate(new Vector3(distance / 100000, 0));
        Singleton.CameraController.MoveTo(new Coordinate(_focusedBounds.center));
        Singleton.CameraController.Zoom(distance);
    }

    private void UpdateCategoryButton(GameObject button, int index, bool isActive)
    {
        button.GetComponent<GrowOnHover>().SetBaseScale(isActive ? 1 : 0);

        var targetPos = new Vector3(0, 0, 0);
        if (index >= 0)
        {
            var theta = ((index + 1f) / CategoryButtons.Count(x => x.IsActive)) * 2f * math.PI;
            targetPos.x = math.sin(theta);
            targetPos.y = math.cos(theta);
            targetPos *= Distance;
        }
        button.transform.localPosition = Vector3.Lerp(button.transform.localPosition, targetPos, OpenSpeed * Time.deltaTime);
    }

    [Serializable]
    public enum UiState
    {
        Disabled,
        Closed,
        CategorySelection,
        Category,
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
    public struct CategoryButton
    {
        public GameObject Button;
        public GeneCategory Category;
        public bool IsActive;
    }
}
