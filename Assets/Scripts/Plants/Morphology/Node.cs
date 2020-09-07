using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node : MonoBehaviour
{
    public Plant Plant { get; set; }
    public Node Base { get; set; }
    public List<Node> Branches { get; set; } = new List<Node>();
    public RenderingInstanceData NodeMesh { get; set; }
    public RenderingInstanceData InternodeMesh { get; set; }
    public NodeDna Dna => Plant.PlantDna.Nodes.FirstOrDefault(x => x.Type == Type) ?? new NodeDna();

    public int CreationDate { get; set; }
    public int Age => Singleton.TimeService.Day - CreationDate;

    public string Type;
    public float Size;
    public float InternodeLength;
    public float InternodeRadius;
    public float SurfaceArea;

    public float AbsorbedLight;
    public float GrowthHormone;
}
