using System;

[Serializable]
public class PlateTectonicsDbData
{
    public string PlanetName;
    public float MantleHeight;
    public PlateDbData[] Plates;
    public EnvironmentMapDbData LandHeightMap;
    public EnvironmentMapDbData ContinentalIdMap;
    public EnvironmentMapDbData PlateThicknessMaps;
    public ToolsDbData[] Tools;
}

[Serializable]
public class PlateDbData
{
    public float Id;
    public int Idx;
    public float[] Rotation;
}