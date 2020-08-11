using System.Collections.Generic;
using UnityEngine;

public class UiController: MonoBehaviour
{
    private UiState _currentState;
    private UiData _data;
    public Plant FocusedPlant;

    public void Start()
    {
        SetState(FindObjectOfType<BasicInfoUi>());
    }

    public void SetState(UiState newState)
    {
        if (_currentState != null && _currentState.Disable(_data))
        {
            if (newState.Enable(_data))
            {
                _currentState = newState;
            }
            else
            {
                _currentState.Enable(_data);
            }
        }
    }

}

public interface UiState
{
    public bool Enable(UiData data);
    public bool Disable(UiData data);
}

public class UiData
{
    public Plant FocusedPlant;
}