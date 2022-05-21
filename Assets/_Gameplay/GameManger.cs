using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManger : MonoBehaviour
{
    public string CurrentPlanet;

    void Awake()
    {
        LoadPlanet(CurrentPlanet);
    }

    void LoadPlanet(string planetName)
    {
        EnvironmentMapDataStore.Load(planetName);

        var plateTectonicsData = SimulationDataStore.LoadPlateTectonicsSimulation(planetName);
        if (plateTectonicsData.Plates.Any())
            FindObjectOfType<PlateTectonicsSimulation>().Initialize(plateTectonicsData);
        else
            FindObjectOfType<LandGenerator>().Regenerate();
    }
}
