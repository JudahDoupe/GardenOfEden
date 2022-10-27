using System;

[Serializable]
public class EnvironmentMapDbData
{
    public EnvironmentMapDbData() { }
    public EnvironmentMapDbData(string planetName, string mapName, int channels = 1, int layers = 6)
    {
        PlanetName = planetName;
        MapName = mapName;
        Channels = channels;
        Layers = layers;
    }

    public string PlanetName;
    public string MapName;
    public int Channels;
    public int Layers;
}