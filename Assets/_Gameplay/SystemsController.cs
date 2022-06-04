using System;
using UnityEngine;

public class SystemsController : MonoBehaviour
{
    private Action _disableCurrentSystem;

    private void Awake()
    {
        _disableCurrentSystem = () => { };
    }

    public void InitializeAllSystems(PlanetData data)
    {
        FindObjectOfType<PlateTectonicsSimulation>().Initialize(data.PlateTectonics);
        FindObjectOfType<PlateTectonicsVisualization>().Initialize(data.PlateTectonics);
        FindObjectOfType<PlateTectonicsAudio>().Initialize(data.PlateTectonics);
        FindObjectOfType<PlateTectonicsToolbar>().Initialize(data.PlateTectonics);
        FindObjectOfType<PlateBaker>().Initialize(data.PlateTectonics);
        
        FindObjectOfType<WaterSimulation>().Initialize(data.Water);
    }

    public void EnablePlateTectonics()
    {
        _disableCurrentSystem();
        FindObjectOfType<PlateTectonicsSimulation>().Enable();
        FindObjectOfType<PlateTectonicsAudio>().Enable();
        FindObjectOfType<PlateTectonicsVisualization>().Enable();
        FindObjectOfType<WaterSimulation>().Enable();
        _disableCurrentSystem = DisablePlateTectonics;
    }  
    public void DisablePlateTectonics()
    {
        SimulationDataStore.UpdatePlateTectonics(Planet.Data.PlateTectonics);
        FindObjectOfType<PlateTectonicsSimulation>().Disable();
        FindObjectOfType<PlateTectonicsAudio>().Disable();
        FindObjectOfType<PlateTectonicsVisualization>().Disable();
        FindObjectOfType<PlateBaker>().Disable();
        FindObjectOfType<PlateBaker>().BakePlates();
        FindObjectOfType<WaterSimulation>().Disable();
    }

    public void EnableGlobe()
    {
        _disableCurrentSystem();
        FindObjectOfType<WaterSimulation>().Enable();
        _disableCurrentSystem = DisableGlobe;
    }
    public void DisableGlobe()
    {
        FindObjectOfType<WaterSimulation>().Disable();
    }
}

public enum SimulationType
{
    PlateTectonics,
    Water,
}
