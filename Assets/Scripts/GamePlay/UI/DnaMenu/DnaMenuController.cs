using Assets.Scripts.Plants.Dna;
using Stateless;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Assets.Scripts.Utils;

public class DnaMenuController : MonoBehaviour
{
    public GameObject OpenMenuButton;
    public GameObject NextCategoryButton;
    public GameObject LastCategoryButton;

    public GameObject PanelPrefab;

    public float DriftSpeed = 5f;

    private StateMachine<UiState, UiTrigger> _stateMachine;
    private StateMachine<UiState, UiTrigger>.TriggerWithParameters<GeneCategory> _selectCategory;

    private UiState _state = UiState.Closed;
    private Entity _focusedPlant;
    private Bounds _focusedBounds;

    private int _currentPanelIndex = 0;
    private List<DnaCategoryPanel> _panels = new List<DnaCategoryPanel>();

    public void Enable() => _stateMachine.Fire(UiTrigger.Enable);
    public void Disable() => _stateMachine.Fire(UiTrigger.Disable);
    public void EditDna() => _stateMachine.Fire(UiTrigger.EditDna);
    public void SelectCategory(GeneCategory category) => _stateMachine.Fire(_selectCategory, category);

    private void Start()
    {
        _stateMachine = new StateMachine<UiState, UiTrigger>(() => _state, s => _state = s);
        _selectCategory = _stateMachine.SetTriggerParameters<GeneCategory>(UiTrigger.SelectCategory);

        _stateMachine.Configure(UiState.Disabled)
            .Permit(UiTrigger.Enable, UiState.Closed);
        _stateMachine.Configure(UiState.Closed)
            .OnEntry(() =>
            {
                OpenMenuButton.SetActive(true);
            })
            .OnExit(() =>
            {
                OpenMenuButton.SetActive(false);
            })
            .Permit(UiTrigger.Disable, UiState.Disabled)
            .Permit(UiTrigger.EditDna, UiState.Open);
        _stateMachine.Configure(UiState.Open)
            .OnEntry(() =>
            {
                var dnaReference = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<DnaReference>(_focusedPlant);
                var dna = DnaService.GetSpeciesDna(dnaReference.SpeciesId);
                foreach (var category in dna.GetGeneCategories())
                {
                    var panel = Instantiate(PanelPrefab, transform).GetComponent<DnaCategoryPanel>();
                    _panels.Add(panel);
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
            })
            .Permit(UiTrigger.SelectCategory, UiState.Carousel);
        _stateMachine.Configure(UiState.Carousel)
            .SubstateOf(UiState.Open)
            .OnEntryFrom(_selectCategory, (category) =>
            {
                _currentPanelIndex = _panels.IndexOf(_panels.Single(x => x.Category == category));
                PositionCarouselPanels();
            })
            .OnEntryFrom(UiTrigger.LastCategory, () =>
            {
                _currentPanelIndex = (_currentPanelIndex + 1) % _panels.Count;
                PositionCarouselPanels();
            })
            .OnEntryFrom(UiTrigger.NextCategory, () =>
            {
                _currentPanelIndex = (_currentPanelIndex - 1) % _panels.Count;
                PositionCarouselPanels();
            })
            .Ignore(UiTrigger.SelectCategory)
            .PermitReentry(UiTrigger.LastCategory)
            .PermitReentry(UiTrigger.NextCategory)
            .Permit(UiTrigger.Close, UiState.Closed);


    }

    private void Update()
    {
        if (!_stateMachine.IsInState(UiState.Open))
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
        Singleton.CameraController.Rotate(new Vector3((distance * DriftSpeed) / 100000, 0));
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
    }

    private void PositionCarouselPanels()
    {
        this.AnimateTransform(0.3f, new Vector3(-300, 0, 0), Vector3.one);

        for (int i = 0; i < _panels.Count; i++)
        {
            var panel = _panels[(i + _currentPanelIndex) % _panels.Count];
            panel.transform.SetSiblingIndex(_panels.Count - (i+1));
            panel.AnimateTransform(0.3f, new Vector3(-25 * i, 0, 0), Vector3.one * (1 - 0.1f * i));
            if (i == 0)
            {
                panel.Activate();
            }
            else
            {
                panel.Deactivate();
            }
        }
    }

    [Serializable]
    public enum UiState
    {
        Disabled,
        Closed,
        Open,
        Carousel,
    }
    [Serializable]
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
