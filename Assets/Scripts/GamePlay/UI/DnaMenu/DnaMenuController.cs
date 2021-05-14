using Assets.Scripts.Plants.Dna;
using Stateless;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using Assets.Scripts.Utils;
using UnityEngine.UI;

public class DnaMenuController : MonoBehaviour
{
    public GameObject OpenMenuButton;
    public Button NextCategoryButton;
    public Button LastCategoryButton;
    public Button DoneButton;

    public GameObject PanelPrefab;

    public float CameraDriftSpeed = 5f;

    private StateMachine<UiState, UiTrigger> _stateMachine;
    private StateMachine<UiState, UiTrigger>.TriggerWithParameters<GeneCategory> _selectCategory;
    private UiState _state = UiState.Closed;

    private Entity _focusedPlant;
    private Bounds _focusedBounds;
    private LinkedList<DnaCategoryPanel> _panels = new LinkedList<DnaCategoryPanel>();


    public void Enable() => _stateMachine.Fire(UiTrigger.Enable);
    public void Disable() => _stateMachine.Fire(UiTrigger.Disable);
    public void EditDna() => _stateMachine.Fire(UiTrigger.EditDna);
    public void SelectCategory(GeneCategory category) => _stateMachine.Fire(_selectCategory, category);
    public void NextCategory() => _stateMachine.Fire(UiTrigger.NextCategory);
    public void LastCategory() => _stateMachine.Fire(UiTrigger.LastCategory);
    public void Done() => _stateMachine.Fire(UiTrigger.Close);
    public void Evolve(Gene evolution)
    {
        Done();
    }


    private void Start()
    {
        _stateMachine = new StateMachine<UiState, UiTrigger>(() => _state, s => _state = s);
        _selectCategory = _stateMachine.SetTriggerParameters<GeneCategory>(UiTrigger.SelectCategory);

        _stateMachine.Configure(UiState.Disabled)
            .Permit(UiTrigger.Enable, UiState.Closed);

        _stateMachine.Configure(UiState.Closed)
            .SubstateOf(UiState.Enabled)
            .OnEntry(() =>
            {
                NextCategoryButton.AnimateTransform(0.3f, Vector3.zero, Vector3.zero, false);
                LastCategoryButton.AnimateTransform(0.3f, Vector3.zero, Vector3.zero, false);
                OpenMenuButton.SetActive(true);
            })
            .OnExit(() =>
            {
                OpenMenuButton.SetActive(false);
            })
            .Permit(UiTrigger.Disable, UiState.Disabled)
            .Permit(UiTrigger.EditDna, UiState.Open);

        _stateMachine.Configure(UiState.Open)
            .SubstateOf(UiState.Enabled)
            .OnEntry(() =>
            {
                var dnaReference = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<DnaReference>(_focusedPlant);
                var dna = DnaService.GetSpeciesDna(dnaReference.SpeciesId);
                foreach (var category in dna.GetGeneCategories())
                {
                    var panel = Instantiate(PanelPrefab, transform).GetComponent<DnaCategoryPanel>();
                    _panels.AddLast(panel);
                    panel.Init(dna, category);
                }
                PositionOpenPanels();
            })
            .OnExit(() =>
            {
                foreach (var panel in _panels)
                {
                    Destroy(panel.gameObject);
                }
                _panels.Clear();
                DoneButton.AnimateTransform(0.3f, new Vector3(0, 0, 0), Vector3.zero, false);
            })
            .Permit(UiTrigger.SelectCategory, UiState.Carousel);

        _stateMachine.Configure(UiState.Carousel)
            .SubstateOf(UiState.Open)
            .OnEntryFrom(_selectCategory, (category) =>
            {
                var panel = _panels.First(x => x.Category == category);
                _panels.Remove(panel);
                _panels.AddFirst(panel);
                PositionCarouselPanels();
            })
            .OnEntryFrom(UiTrigger.LastCategory, () =>
            {
                var panel = _panels.Last;
                _panels.Remove(panel);
                _panels.AddFirst(panel);
                PositionCarouselPanels();
            })
            .OnEntryFrom(UiTrigger.NextCategory, () =>
            {
                var panel = _panels.First;
                _panels.Remove(panel);
                _panels.AddLast(panel);
                PositionCarouselPanels();
            })
            .Ignore(UiTrigger.SelectCategory)
            .PermitReentry(UiTrigger.LastCategory)
            .PermitReentry(UiTrigger.NextCategory)
            .Permit(UiTrigger.Close, UiState.Closed);


    }
    private void Update()
    {
        if (_stateMachine.IsInState(UiState.Enabled))
        {
            _focusedPlant = CameraUtils.GetClosestEntity(Singleton.CameraController.FocusPos);
            if (_focusedPlant != Entity.Null)
            {
                _focusedBounds = CameraUtils.EncapsulateChildren(_focusedPlant);
                var direction = Vector3.Normalize(_focusedBounds.center);
                OpenMenuButton.transform.position = _focusedBounds.ClosestPoint(2 * _focusedBounds.center) + direction;
            }
        }

        if (_stateMachine.IsInState(UiState.Open))
        {
            DriftCamera();
        }
    }

    private void DriftCamera()
    {
        var distance = Mathf.Clamp(CameraUtils.GetDistanceToIncludeBounds(_focusedBounds, 1.5f), 5, 25);
        Singleton.CameraController.Rotate(new Vector3((distance * CameraDriftSpeed) / 100000, 0));
        Singleton.CameraController.MoveTo(new Coordinate(_focusedBounds.center));
        Singleton.CameraController.Zoom(distance);
    }

    private void PositionOpenPanels()
    {
        this.AnimateTransform(0.3f, Vector3.zero, Vector3.one);

        var positions = new Stack<Vector3>();
        for (int i = 0; i < _panels.Count; i++)
        {
            var offset = (i - (_panels.Count - 1) / 2f) * 300;
            positions.Push(new Vector3(offset, 0));
        }

        foreach (var panel in _panels)
        {
            panel.AnimateTransform(0.3f, positions.Pop(), Vector3.one);
            panel.Activate();
        }

        DoneButton.gameObject.SetActive(true);
        DoneButton.transform.SetSiblingIndex(_panels.Count);
        DoneButton.GetComponent<Button>().AnimateTransform(0.3f, new Vector3(0, -350, 0), Vector3.one);
    }
    private void PositionCarouselPanels()
    {
        this.AnimateTransform(0.3f, new Vector3(-300, 0, 0), Vector3.one);

        var i = 0;
        foreach (var panel in _panels)
        {
            panel.transform.SetSiblingIndex(0);
            panel.AnimateTransform(0, new Vector3(-25 * i, -25 * i, 0), Vector3.one * (1 - 0.1f * i));
            if (i == 0)
            {
                panel.Activate();
                var rect = panel.GetComponent<RectTransform>().rect;
                NextCategoryButton.gameObject.SetActive(true);
                NextCategoryButton.transform.SetSiblingIndex(_panels.Count + 1);
                NextCategoryButton.AnimateTransform(0.3f, new Vector3(-(rect.width / 2f) - 10, (rect.height / 2f) + 10, 0), Vector3.one);
                LastCategoryButton.gameObject.SetActive(true);
                LastCategoryButton.transform.SetSiblingIndex(_panels.Count + 2);
                LastCategoryButton.AnimateTransform(0.3f, new Vector3((rect.width / 2f) + 10, (rect.height / 2f) + 10, 0), Vector3.one);
            }
            else
            {
                panel.Deactivate();
            }

            i++;
        }

    }
    

    public enum UiState
    {
        Disabled,
        Enabled,
        Closed,
        Open,
        Carousel,
    }
    public enum UiTrigger
    {
        Enable,
        Disable,
        EditDna,
        Close,
        SelectCategory,
        NextCategory,
        LastCategory,
    }
}
