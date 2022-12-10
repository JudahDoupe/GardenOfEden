using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        FindObjectOfType<PlateBakerV2>().Initialize(data.PlateTectonics);
        FindObjectOfType<MovePlateTool>().Initialize(data.PlateTectonics);
        FindObjectOfType<BreakPlateTool>().Initialize(data.PlateTectonics);
        FindObjectOfType<MergePlateTool>().Initialize(data.PlateTectonics);

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
        FindObjectOfType<PlateTectonicsSimulation>().Disable();
        FindObjectOfType<PlateTectonicsAudio>().Disable();
        FindObjectOfType<PlateTectonicsVisualization>().Disable();
        FindObjectOfType<WaterSimulation>().Disable();

        Planet.Instance.RunTaskInCoroutine(Task.WhenAll(new List<Task>
        {
            SimulationDataStore.UpdatePlateTectonics(Planet.Data.PlateTectonics),
            SimulationDataStore.UpdateWater(Planet.Data.Water),
        }));
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
