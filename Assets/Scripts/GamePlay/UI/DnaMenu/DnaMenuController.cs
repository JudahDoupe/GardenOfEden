using Assets.Scripts.Plants.Dna;
using Stateless;
using System;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public class DnaMenuController : MonoBehaviour
{
    public GameObject OpenMenuButton;
    public CategoryButton[] CategoryButtons;
    public float OpenSpeed = 1f;
    public float Distance = 3f;

    public StateMachine<UiState, UiTrigger> StateMachine { get; private set; }

    private UiState _state = UiState.Closed;
    private Entity _focusedPlant;
    private Bounds _focusedBounds;
    private Dna _dna;
    private GeneCategory _category;
    private string _gene;

    public void Enable() => StateMachine.Fire(UiTrigger.Enable);
    public void Disable() => StateMachine.Fire(UiTrigger.Disable);
    public void EditDna() => StateMachine.Fire(UiTrigger.EditDna);
    public void SelectCategory(GeneCategory category) => StateMachine.Fire(UiTrigger.EditDna);

    private void Start()
    {
        StateMachine = new StateMachine<UiState, UiTrigger>(() => _state, s => _state = s);

        StateMachine.SetTriggerParameters<GeneCategory>(UiTrigger.SelectCategory);
        StateMachine.SetTriggerParameters<string>(UiTrigger.SelectGene);

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
                    CategoryButtons[i].IsActive = _dna.GeneTypes.Contains(CategoryButtons[i].Type);
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

        if (StateMachine.IsInState(UiState.CategorySelection))
        {
            var lastIndex = -1;
            for (int i = 0; i < CategoryButtons.Length; i++)
            {
                var index = CategoryButtons[i].IsActive ? ++lastIndex : -1;
                UpdateCategoryButton(CategoryButtons[i].Button, index, CategoryButtons[i].IsActive);
            }
        }
    }

    private void DriftCamera()
    {
        var distance = CameraUtils.GetDistanceToIncludeBounds(_focusedBounds, 2.5f);
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
    }
    [Serializable]
    public enum UiTrigger
    {
        Enable,
        Disable,
        EditDna,
        Close,
        SelectCategory,
        SelectGene,
    }

    [Serializable]
    public struct CategoryButton
    {
        public GameObject Button;
        public GeneType Type;
        public bool IsActive;
    }
}
