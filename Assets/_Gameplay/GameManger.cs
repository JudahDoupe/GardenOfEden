using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManger : MonoBehaviour
{
    public string CurrentPlanet;

    void Awake()
    {
        EnvironmentMapDataStore.Load(CurrentPlanet);
    }
}
