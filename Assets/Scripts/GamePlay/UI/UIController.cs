using UnityEngine;

public class UiController: MonoBehaviour
{
    public UiState CurrentState;
    public UiData Data = new UiData();

    public void Start()
    {
        SetState(FindObjectOfType<BasicInfoUi>());
    }

    public void SetState(UiState newState)
    {
        if (CurrentState != null && !CurrentState.Disable(Data))
        {
            CurrentState.Enable(Data);
        }
        else
        {
            if (newState.Enable(Data))
            {
                CurrentState = newState;
            }
            else
            {
                CurrentState.Enable(Data);
            }
        }
    }

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