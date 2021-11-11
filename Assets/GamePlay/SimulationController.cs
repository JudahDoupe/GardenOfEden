using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    private static Dictionary<SimulationType, ISimulation> _simulations = new Dictionary<SimulationType, ISimulation>(); 
    private void Awake()
    {
        _simulations.Add(SimulationType.PlateTectonics, FindObjectOfType<PlateTectonicsSimulation>());
        _simulations.Add(SimulationType.Water, FindObjectOfType<WaterSimulation>());
    }

    public static void StartSimulations(params SimulationType[] sims)
    {
        foreach (var sim in sims)
        {
            _simulations[sim].IsActive = true;
        }
    }
    public static void StopSimulations(params SimulationType[] sims)
    {
        foreach (var sim in sims)
        {
            _simulations[sim].IsActive = false;
        }
    }
    public static void SetActiveSimulations(params SimulationType[] sims)
    {
        foreach (var sim in _simulations)
        {
            sim.Value.IsActive = sims.Contains(sim.Key);
        }
    }
    public static bool IsSimulationRunning(SimulationType simulation) => _simulations[simulation].IsActive;
}

public enum SimulationType
{
    PlateTectonics,
    Water,
}
