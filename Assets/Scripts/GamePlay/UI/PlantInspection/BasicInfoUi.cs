using UnityEngine;

public class BasicInfoUi : MonoBehaviour, UiState
{
    public bool Enable(UiData data)
    {
        return true;
    }

    public bool Disable(UiData data)
    {
        return true;
    }

    public void ShowDescription()
    {

    }

    public void HideDescription()
    {

    }
}
