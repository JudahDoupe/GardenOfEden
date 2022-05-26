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

    public string PlanetName { get; set; }
    public string MapName { get; set; }
    public int Channels { get; set; }
    public int Layers { get; set; }
}