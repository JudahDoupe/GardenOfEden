using Assets.Scripts.Plants.Dna;
using Stateless;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using Assets.Scripts.Utils;
using Assets.Utils;
using Unity.Mathematics;
using UnityEngine.UI;
using System.Collections;
using Assets.GamePlay.Cameras;

public class DnaUi : MonoBehaviour
{
    public GameObject CarouselControls;
    public Button DoneButton;

    public GameObject PanelPrefab;

    private StateMachine<UiState, UiTrigger> _stateMachine;
    private StateMachine<UiState, UiTrigger>.TriggerWithParameters<GeneCategory> _selectCategory;
    private UiState _state = UiState.Closed;

    private Entity _focusedPlant;
    private Bounds _focusedBounds;
    private LinkedList<DnaCategoryPanel> _panels = new LinkedList<DnaCategoryPanel>();


    public void EditDna(Entity plant)
    {
        _focusedPlant = plant;
        _stateMachine.Fire(UiTrigger.EditDna);
    }
    public void Flatten() => _stateMachine.Fire(UiTrigger.Flatten);
    public void SelectCategory(GeneCategory category) => _stateMachine.Fire(_selectCategory, category);
    public void NextCategory() => _stateMachine.Fire(UiTrigger.NextCategory);
    public void LastCategory() => _stateMachine.Fire(UiTrigger.LastCategory);
    public void Done()
    {
        _stateMachine.Fire(UiTrigger.Close);
        StartCoroutine(Exit());
    }
    public void Evolve(Gene evolution)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var coordinate = em.GetComponentData<Coordinate>(_focusedPlant);
        var dna = DnaService.GetSpeciesDna(em.GetComponentData<DnaReference>(_focusedPlant).SpeciesId).Evolve(evolution);
        EcsUtils.DestroyAllChildren(_focusedPlant);
        _focusedPlant = dna.Spawn(coordinate);
        Done();
    }
    private IEnumerator Exit()
    {
        yield return new WaitForEndOfFrame();

        var perspective = FindObjectOfType<ObservationCamera>();
        Singleton.PerspectiveController.SetPerspective(perspective, CameraTransition.Instant);
    }

    private void Start()
    {
        _stateMachine = new StateMachine<UiState, UiTrigger>(() => _state, s => _state = s);
        _selectCategory = _stateMachine.SetTriggerParameters<GeneCategory>(UiTrigger.SelectCategory);

        _stateMachine.Configure(UiState.Closed)
            .Permit(UiTrigger.EditDna, UiState.Flat)
            .Ignore(UiTrigger.Close);

        _stateMachine.Configure(UiState.Open)
            .OnEntryFrom(UiTrigger.EditDna, () =>
            {
                var dnaReference = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<DnaReference>(_focusedPlant);
                var dna = DnaService.GetSpeciesDna(dnaReference.SpeciesId);
                foreach (var category in dna.GetGeneCategories())
                {
                    var panel = Instantiate(PanelPrefab, transform).GetComponent<DnaCategoryPanel>();
                    _panels.AddLast(panel);
                    panel.Init(dna, category);
                }

                DoneButton.gameObject.SetActive(true);
                DoneButton.transform.SetSiblingIndex(_panels.Count);
                DoneButton.transform.AnimateTransform(0.3f, new Vector3(0, -350, 0), Vector3.one);
            })
            .OnExit(() =>
            {
                foreach (var panel in _panels)
                {
                    Destroy(panel.gameObject);
                }

                _panels.Clear();
                DoneButton.transform.AnimateTransform(0.3f, new Vector3(0, 0, 0), Vector3.zero, false);
                CarouselControls.transform.AnimateTransform(0.3f, new Vector3(0, 0, 0), Vector3.zero, false);
            })
            .Permit(UiTrigger.Close, UiState.Closed);

        _stateMachine.Configure(UiState.Flat)
            .SubstateOf(UiState.Open)
            .OnEntry(PositionOpenPanels)
            .Permit(UiTrigger.Close, UiState.Closed)
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
            .Permit(UiTrigger.Flatten, UiState.Flat)
            .Permit(UiTrigger.Close, UiState.Closed);


    }

    private void PositionOpenPanels()
    {
        var positions = new Stack<Vector3>();
        for (int i = 0; i < _panels.Count; i++)
        {
            var offset = (i - (_panels.Count - 1) / 2f) * 300;
            positions.Push(new Vector3(offset, 0));
        }

        foreach (var panel in _panels)
        {
            panel.transform.AnimateUiOpacity(0, 1);
            panel.transform.AnimateTransform(0.3f, positions.Pop(), Vector3.one);
            panel.Deactivate();
            panel.Activate();
        }

        CarouselControls.SetActive(false);
    }
    private void PositionCarouselPanels()
    {
        float i = 0;
        foreach (var panel in _panels)
        {
            var x = 2f * (i / _panels.Count) * math.PI;
            var cos = (math.cos(x) + 1f) / 2f;
            var sin = (math.sin(x) + 1f) / 2f;

            var position = new Vector3(-400 + (0.5f - sin) * 150, (1 + (1 - cos) * -15), 0);
            var scale = Vector3.one * (1 + (1 - cos) * -0.2f);
            var opacity = cos * cos;

            panel.transform.SetSiblingIndex(0);
            panel.transform.AnimateUiOpacity(0, opacity);
            panel.transform.AnimateTransform(0.1f, position, scale);
            if (i == 0)
            {
                panel.Activate();
                var rect = panel.GetComponent<RectTransform>().rect;
                CarouselControls.SetActive(true);
                CarouselControls.transform.AnimateTransform(0.1f, new Vector3(-400, (rect.height / 2f) + 60, 0), Vector3.one);
            }
            else
            {
                panel.Deactivate();
            }

            i+=1;
        }

    }
    

    public enum UiState
    {
        Closed,
        Open,
        Carousel,
        Flat,
    }
    public enum UiTrigger
    {
        EditDna,
        Close,
        Flatten,
        SelectCategory,
        NextCategory,
        LastCategory,
    }
}
