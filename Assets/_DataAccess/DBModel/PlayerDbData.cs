using System;
using UnityEngine.Serialization;

[Serializable]
public class PlayerDbData
{
    public string CurrentPlanetName;
    public string[] PlanetNames;
    public SettingsDbData Settings;
}

[Serializable]
public class SettingsDbData
{
    public float ScrollSpeed;
    public float DragSpeed;
}