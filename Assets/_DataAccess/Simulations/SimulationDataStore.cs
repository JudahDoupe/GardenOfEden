using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SimulationDataStore
{
    private static string ConnectionString(string planetName) => $@"{Application.persistentDataPath}\{planetName}\Simulations.db";

    public static void SavePlateTectonicsSimulation(PlateTectonicsSimulationData data, string planetName)
    {

    }

    public static PlateTectonicsSimulationData LoadPlateTectonicsSimulation(string planetName)
    {

    }
}
