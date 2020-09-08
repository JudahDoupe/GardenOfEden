using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiController: MonoBehaviour
{
    public UiData Data = new UiData();
    public UiStateType CurrentStateType { get; private set; }
    public UiState CurrentState { get; private set; }

    private Dictionary<UiStateType, UiState> _states = new Dictionary<UiStateType, UiState>();

    public void Start()
    {
        _states.Add(UiStateType.BasicInfo, FindObjectOfType<BasicInfoUi>());
        _states.Add(UiStateType.Evolution, FindObjectOfType<PlantEvolutionUi>());
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetState(CurrentStateType == UiStateType.None ? UiStateType.BasicInfo : UiStateType.None);
        }

        if (Data.FocusedPlant != null && Input.GetKeyDown(KeyCode.E))
        {
            SetState(UiStateType.Evolution);
        }
    }

    public void SetState(UiStateType newStateType)
    {
        if (CurrentState != null && !CurrentState.Disable(Data))
        {
            CurrentState.Enable(Data);
        }
        else
        {
            if (_states.TryGetValue(newStateType, out var newState))
            {
                newState.Enable(Data);
            }
            CurrentStateType = newStateType;
            CurrentState = newState;
        }
    }
}

public enum UiStateType
{
    None,
    BasicInfo,
    Evolution,
}

public interface UiState
{
    bool Enable(UiData data);
    bool Disable(UiData data);
}

public class UiData
{
    public Plant FocusedPlant;
}