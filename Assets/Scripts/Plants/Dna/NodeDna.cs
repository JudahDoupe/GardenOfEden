using System.Collections.Generic;

public class NodeDna
{
    public string Type;
    public string MeshId; 
    public float Size;
    public float InternodeLength;
    public float InternodeRadius;
    public float LightAbsorbtionRate;
    public List<GrowthRule> GrowthRules = new List<GrowthRule>();
}