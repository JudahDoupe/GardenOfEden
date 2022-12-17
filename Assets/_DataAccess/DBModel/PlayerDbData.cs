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
    [FormerlySerializedAs("SCrollSpeed")] [FormerlySerializedAs("ZoomSpeed")] public float ScrollSpeed;
    [FormerlySerializedAs("ScrollSpeed")] public float DragSpeed;
}
