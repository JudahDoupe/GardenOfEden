public class PlateTectonicsDbData
{
    public string PlanetName { get; set; }
    public float MantleHeight { get; set; }
    public PlateDbData[] Plates { get; set; }
    public EnvironmentMapDbData LandHeightMap { get; set; }
    public EnvironmentMapDbData ContinentalIdMap { get; set; } 
    public EnvironmentMapDbData PlateThicknessMaps { get; set; }
}

public class PlateDbData
{
    public float Id { get; set; }
    public int Idx { get; set; }
    public float[] Rotation { get; set; }
}