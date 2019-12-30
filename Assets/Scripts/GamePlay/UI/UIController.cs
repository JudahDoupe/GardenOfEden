using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private GameObject _pauseMenu;
    private GameObject _statsMenu;
    private GameObject _evolutionMenu;

    private GameService _gameService;

    private void Start()
    {
        _pauseMenu = transform.Find("Pause Menu").gameObject;
        _statsMenu = transform.Find("Stats").gameObject;
        _evolutionMenu = transform.Find("Evolution").gameObject;

        _gameService = FindObjectOfType<GameService>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_pauseMenu.activeSelf)
            {
                HidePauseMenu();
            }
            else
            {
                ShowPauseMenu();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_evolutionMenu.activeSelf)
            {
                HideEvolutionMenu();
            }
            else
            {
                ShowEvolutionMenu(_gameService.FocusedPlant);
            }
        }
    }

    public void ShowPauseMenu()
    {
        _pauseMenu.SetActive(true);
    }
    public void HidePauseMenu()
    {
        _pauseMenu.SetActive(false);
    }

    public void ShowStatsMenu()
    {
        _statsMenu.transform.Find("Plants").transform.Find("Text").GetComponent<Text>().text = PlantApi.GetTotalPlantPopulation().ToString();
        _statsMenu.SetActive(true);
    }
    public void HideStatsMenu()
    {
        _statsMenu.SetActive(false);
    }

    public void ShowEvolutionMenu(Plant plant)
    {
        _evolutionMenu.SetActive(true);
        _evolutionMenu.GetComponent<EvolutionUI>().Enable(plant);
    }
    public void HideEvolutionMenu()
    {
        _evolutionMenu.SetActive(false);
        _evolutionMenu.GetComponent<EvolutionUI>().Disable();
    }
}
