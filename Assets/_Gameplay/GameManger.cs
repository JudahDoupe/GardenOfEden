using System.Collections;
using System.Collections.Generic;
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
        FindObjectOfType<PlateTectonicsSimulation>().Initialize(SimulationDataStore.LoadPlateTectonicsSimulation(planetName));
    }
}
