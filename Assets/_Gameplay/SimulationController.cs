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
            _simulations[sim].Enable();
        }
    }
    public static void StopSimulations(params SimulationType[] sims)
    {
        foreach (var sim in sims)
        {
            _simulations[sim].Disable();
        }
    }
    public static void SetEnabledSimulations(bool isEnabled, params SimulationType[] sims)
    {
        foreach (var sim in _simulations.Where(x => sims.Contains(x.Key)))
        {
            if (isEnabled)
                sim.Value.Enable();
            else
                sim.Value.Disable();
        }
    }
    public static bool IsSimulationRunning(SimulationType simulation) => _simulations[simulation].IsActive;
}

public enum SimulationType
{
    PlateTectonics,
    Water,
}
