using UnityEngine;
using UnityEngine.UI;

public class BasicInfoUi : MonoBehaviour, IUiState
{
    public CameraController Controller;
    public Text Title;
    public Text Description;

    public void Update()
    {
        if (GetComponent<Canvas>().enabled)
        {
            if (Controller.FocusedPlant == null)
            {
                Title.text = "";
                Description.text = "";
            }
            else
            {
                Title.text = Controller.FocusedPlant.PlantDna.Name;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Controller.UiState.SetState(FindObjectOfType<CinematicUi>());
            }
        }
    }

    public void Enable()
    {
        GetComponent<Canvas>().enabled = true;
    }

    public void Disable()
    {
        GetComponent<Canvas>().enabled = false;
    }

    public void ShowDescription()
    {

    }

    public void HideDescription()
    {

    }
}
