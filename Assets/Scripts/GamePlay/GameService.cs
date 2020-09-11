using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameService : MonoBehaviour
{
    public bool IsGameInProgress { get; private set; }

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        IsGameInProgress = true;

        Singleton.WorldService.LoadWorld();

        if (!FindObjectsOfType<Plant>().Any())
        {
            var dna = new PlantDna()
            {
                Genes = new List<PlantGene>()
                {
                    GeneCache.GetGenesInCategory(PlantGeneCategory.EnergyProduction).First(),
                    GeneCache.GetGenesInCategory(PlantGeneCategory.Reproduction).First(),
                    GeneCache.GetGenesInCategory(PlantGeneCategory.Vegatation).First(),
                }
            };
            PlantFactory.Build(dna, Singleton.CameraController.FocusPoint);
        }
    }

    private void EndGame()
    {
        IsGameInProgress = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
