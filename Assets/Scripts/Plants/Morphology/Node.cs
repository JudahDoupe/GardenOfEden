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
    public PlantDna.NodeDna Dna => Plant.PlantDna.Nodes.FirstOrDefault(x => x.Type == Type) ?? new PlantDna.NodeDna();

    public float CreationDate { get; set; }
    public float LastUpdateDate { get; set; }
    public float Age => EnvironmentApi.GetDate() - CreationDate;

    public string Type;
    public float Size;
    public float InternodeLength;
    public float InternodeRadius;
}
