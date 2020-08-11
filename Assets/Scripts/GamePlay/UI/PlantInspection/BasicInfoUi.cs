using UnityEngine;
using UnityEngine.UI;

public class BasicInfoUi : MonoBehaviour, UiState
{
    public Text Title;
    public Text Description;
    private UiData Data;
    private bool Enabled = false;

    public void Update()
    {
        if (Enabled)
        {
            Title.text = Data?.FocusedPlant?.PlantDna.Name ?? "";
            Description.text = "";
        }
    }

    public bool Enable(UiData data)
    {
        Data = data;
        GetComponent<Canvas>().enabled = true;
        Enabled = true;
        return true;
    }

    public bool Disable(UiData data)
    {
        Enabled = false;
        GetComponent<Canvas>().enabled = false;
        return true;
    }

    public void ShowDescription()
    {

    }

    public void HideDescription()
    {

    }
}
